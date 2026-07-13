using FluentValidation;
using Serilog;
using SqlSugar;
using WF.MES.Core.Constants;
using WF.MES.Core.Exceptions;
using WF.MES.Core.Extensions;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;
using WF.MES.Models.Entities;
using WF.MES.Infrastructure.Validation;

namespace WF.MES.Infrastructure.Services.Barcode;

/// <summary>
/// 条码资料审核实现。附件存 Barcode_MaterialRule 二进制列；上传/确认/驳回经 IMenuActionAuthorization 校验。
/// 规则修改后由 MaterialBarcodeRuleService 经 BarcodeAuditHelper.ResetQaApproval 重置流程。
/// </summary>
public class BarcodeQaReviewService : IBarcodeQaReviewService
{
    private readonly ISqlSugarClient _db;
    private readonly ISessionService _sessionService;
    private readonly IMenuActionAuthorization _authorization;
    private readonly IValidator<BarcodeQaReviewSaveAttachmentsDto> _saveAttachmentsValidator;
    private readonly IValidator<BarcodeQaReviewRejectDto> _rejectValidator;

    public BarcodeQaReviewService(
        ISqlSugarClient db,
        ISessionService sessionService,
        IMenuActionAuthorization authorization,
        IValidator<BarcodeQaReviewSaveAttachmentsDto> saveAttachmentsValidator,
        IValidator<BarcodeQaReviewRejectDto> rejectValidator)
    {
        _db = db;
        _sessionService = sessionService;
        _authorization = authorization;
        _saveAttachmentsValidator = saveAttachmentsValidator;
        _rejectValidator = rejectValidator;
    }

    public async Task<IReadOnlyList<BarcodeQaReviewListDto>> GetListAsync(
        int? qaStatusFilter = null,
        int? customerIdFilter = null,
        string? materialNoFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Queryable<BarcodeMaterialRule, BarcodeCustomer>(
                (rule, customer) => new JoinQueryInfos(
                    JoinType.Inner, rule.CustomerId == customer.CustomerId))
            .Where((rule, customer) => customer.Enable == 1);

        if (qaStatusFilter is >= 0)
        {
            query = query.Where((rule, customer) => rule.QaStatus == qaStatusFilter.Value);
        }

        if (customerIdFilter is > 0)
        {
            query = query.Where((rule, customer) => rule.CustomerId == customerIdFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(materialNoFilter))
        {
            var materialNo = materialNoFilter.Trim();
            query = query.Where((rule, customer) => rule.MaterialNo.Contains(materialNo));
        }

        var rows = await query
            .OrderBy((rule, customer) => customer.CustomerName)
            .OrderBy((rule, customer) => rule.MaterialNo)
            .Select((rule, customer) => new
            {
                rule.RuleId,
                rule.CustomerId,
                customer.CustomerName,
                rule.MaterialNo,
                rule.BarcodeLength,
                rule.QaStatus,
                rule.AttachmentUploadedBy,
                rule.AttachmentUploadedAt,
                rule.QaReviewedBy,
                rule.QaReviewedAt,
                rule.QaReviewRemark
            })
            .ToListAsync();

        if (rows.Count == 0)
        {
            return [];
        }

        var ruleIds = rows.Select(r => r.RuleId).ToList();
        var imagePresenceMap = await GetImagePresenceMapAsync(ruleIds);

        return rows.Select(row =>
        {
            imagePresenceMap.TryGetValue(row.RuleId, out var imagePresence);
            return new BarcodeQaReviewListDto
            {
                RuleId = row.RuleId,
                CustomerId = row.CustomerId,
                CustomerName = row.CustomerName,
                MaterialNo = row.MaterialNo,
                BarcodeLength = row.BarcodeLength,
                QaStatus = row.QaStatus,
                AttachmentUploadedBy = row.AttachmentUploadedBy,
                AttachmentUploadedAt = row.AttachmentUploadedAt,
                QaReviewedBy = row.QaReviewedBy,
                QaReviewedAt = row.QaReviewedAt,
                QaReviewRemark = row.QaReviewRemark,
                HasDrawingImage = imagePresence.HasDrawing,
                HasPrintSampleImage = imagePresence.HasPrintSample
            };
        }).ToList();
    }

    public async Task<BarcodeQaReviewDetailDto?> GetDetailAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        if (ruleId <= 0)
        {
            return null;
        }

        var rows = await _db.Queryable<BarcodeMaterialRule, BarcodeCustomer>(
                (rule, customer) => new JoinQueryInfos(
                    JoinType.Inner, rule.CustomerId == customer.CustomerId))
            .Where((rule, customer) => rule.RuleId == ruleId && customer.Enable == 1)
            .Select((rule, customer) => new
            {
                rule.RuleId,
                rule.CustomerId,
                customer.CustomerName,
                rule.MaterialNo,
                rule.BarcodeLength,
                rule.QaStatus,
                rule.AttachmentUploadedBy,
                rule.AttachmentUploadedAt,
                rule.QaReviewedBy,
                rule.QaReviewedAt,
                rule.QaReviewRemark,
                rule.DrawingImage,
                rule.PrintSampleImage
            })
            .ToListAsync();

        if (rows.Count == 0)
        {
            return null;
        }

        var row = rows[0];

        return new BarcodeQaReviewDetailDto
        {
            RuleId = row.RuleId,
            CustomerId = row.CustomerId,
            CustomerName = row.CustomerName,
            MaterialNo = row.MaterialNo,
            BarcodeLength = row.BarcodeLength,
            QaStatus = row.QaStatus,
            AttachmentUploadedBy = row.AttachmentUploadedBy,
            AttachmentUploadedAt = row.AttachmentUploadedAt,
            QaReviewedBy = row.QaReviewedBy,
            QaReviewedAt = row.QaReviewedAt,
            QaReviewRemark = row.QaReviewRemark,
            DrawingImage = row.DrawingImage,
            PrintSampleImage = row.PrintSampleImage
        };
    }

    public async Task SaveAttachmentsAsync(
        BarcodeQaReviewSaveAttachmentsDto dto,
        CancellationToken cancellationToken = default)
    {
        _authorization.EnsureAction(MenuActions.BarcodeQaReview.SaveAttachments);
        await _saveAttachmentsValidator.ValidateRequestAsync(dto, cancellationToken);

        var rule = await _db.Queryable<BarcodeMaterialRule>().InSingleAsync(dto.RuleId)
            ?? throw new BusinessException("err.materialRuleNotFound");

        if (!BarcodeQaStatus.CanUpload(rule.QaStatus))
        {
            throw new BusinessException("err.uploadNotAllowed");
        }

        BarcodeQaReviewImageHelper.ValidateImageBytes(dto.DrawingImage);
        BarcodeQaReviewImageHelper.ValidateImageBytes(dto.PrintSampleImage);

        if (dto.DrawingImage is { Length: > 0 })
        {
            rule.DrawingImage = dto.DrawingImage;
        }

        if (dto.PrintSampleImage is { Length: > 0 })
        {
            rule.PrintSampleImage = dto.PrintSampleImage;
        }

        if (rule.DrawingImage is not { Length: > 0 } || rule.PrintSampleImage is not { Length: > 0 })
        {
            throw new BusinessException("err.attachmentsBothRequired");
        }

        var operatorName = BarcodeAuditHelper.GetCurrentOperator(_sessionService);
        // 双图齐全后 Qa_Status → 待审核（ApplyQaAttachmentsSaved）
        BarcodeAuditHelper.ApplyQaAttachmentsSaved(rule, operatorName);
        await _db.Updateable(rule).ExecuteCommandAsync();
        Log.Information("料号 {MaterialNo} 资料审核 附件已保存，状态 {QaStatus}", rule.MaterialNo, rule.QaStatus);
    }

    public async Task SaveAttachmentsFromFilesAsync(
        int ruleId,
        string? drawingFilePath,
        string? printSampleFilePath,
        CancellationToken cancellationToken = default)
    {
        byte[]? drawingImage = null;
        byte[]? printSampleImage = null;

        if (!string.IsNullOrWhiteSpace(drawingFilePath))
        {
            drawingImage = BarcodeQaReviewImageHelper.PrepareImageFromFile(drawingFilePath);
        }

        if (!string.IsNullOrWhiteSpace(printSampleFilePath))
        {
            printSampleImage = BarcodeQaReviewImageHelper.PrepareImageFromFile(printSampleFilePath);
        }

        await SaveAttachmentsAsync(new BarcodeQaReviewSaveAttachmentsDto
        {
            RuleId = ruleId,
            DrawingImage = drawingImage,
            PrintSampleImage = printSampleImage
        }, cancellationToken);
    }

    public async Task ApproveAsync(int ruleId, CancellationToken cancellationToken = default)
    {
        _authorization.EnsureAction(MenuActions.BarcodeQaReview.Review);
        var rule = await _db.Queryable<BarcodeMaterialRule>().InSingleAsync(ruleId)
            ?? throw new BusinessException("err.materialRuleNotFound");

        if (!BarcodeQaStatus.CanReview(rule.QaStatus))
        {
            throw new BusinessException("err.qaApproveStateInvalid");
        }

        if (rule.DrawingImage is not { Length: > 0 } || rule.PrintSampleImage is not { Length: > 0 })
        {
            throw new BusinessException("err.attachmentsRequiredForReview");
        }

        var operatorName = BarcodeAuditHelper.GetCurrentOperator(_sessionService);
        BarcodeAuditHelper.ApplyQaApproval(rule, operatorName);
        await _db.Updateable(rule).ExecuteCommandAsync();
        Log.Information("料号 {MaterialNo} 资料审核 已确认", rule.MaterialNo);
    }

    public async Task RejectAsync(BarcodeQaReviewRejectDto dto, CancellationToken cancellationToken = default)
    {
        _authorization.EnsureAction(MenuActions.BarcodeQaReview.Review);
        await _rejectValidator.ValidateRequestAsync(dto, cancellationToken);

        var rule = await _db.Queryable<BarcodeMaterialRule>().InSingleAsync(dto.RuleId)
            ?? throw new BusinessException("err.materialRuleNotFound");

        if (!BarcodeQaStatus.CanReview(rule.QaStatus))
        {
            throw new BusinessException("err.qaRejectStateInvalid");
        }

        var operatorName = BarcodeAuditHelper.GetCurrentOperator(_sessionService);
        BarcodeAuditHelper.ApplyQaRejection(rule, operatorName, dto.RejectRemark);
        await _db.Updateable(rule).ExecuteCommandAsync();
        Log.Information("料号 {MaterialNo} 资料审核 已驳回", rule.MaterialNo);
    }

    private async Task<Dictionary<int, (bool HasDrawing, bool HasPrintSample)>> GetImagePresenceMapAsync(
        IReadOnlyList<int> ruleIds)
    {
        if (ruleIds.Count == 0)
        {
            return [];
        }

        var placeholders = string.Join(", ", ruleIds.Select((_, index) => $"@RuleId{index}"));
        var parameters = ruleIds
            .Select((id, index) => new SugarParameter($"@RuleId{index}", id))
            .Cast<SugarParameter>()
            .ToArray();

        var sql = $@"
SELECT Rule_Id AS RuleId,
       CASE WHEN Drawing_Image IS NOT NULL AND DATALENGTH(Drawing_Image) > 0 THEN 1 ELSE 0 END AS HasDrawingImage,
       CASE WHEN Print_Sample_Image IS NOT NULL AND DATALENGTH(Print_Sample_Image) > 0 THEN 1 ELSE 0 END AS HasPrintSampleImage
FROM dbo.Barcode_MaterialRule
WHERE Rule_Id IN ({placeholders})";

        var imageRows = await _db.Ado.SqlQueryAsync<ImagePresenceRow>(sql, parameters);
        return imageRows.ToDictionary(
            row => row.RuleId,
            row => (row.HasDrawingImage == 1, row.HasPrintSampleImage == 1));
    }

    private sealed class ImagePresenceRow
    {
        public int RuleId { get; set; }

        public int HasDrawingImage { get; set; }

        public int HasPrintSampleImage { get; set; }
    }
}
