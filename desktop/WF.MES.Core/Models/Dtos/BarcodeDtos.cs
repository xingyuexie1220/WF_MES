using System.Text.Json;
using WF.MES.Core.Constants;

namespace WF.MES.Models.Dtos;

/// <summary>客户列表行。</summary>
public class CustomerListDto : IAuditFieldsDto
{
    public int CustomerId { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public int Enable { get; init; }

    public string EnableText => Enable == 1 ? "启用" : "禁用";

    public string? CreatedBy { get; init; }

    public DateTime? CreateDate { get; init; }

    public string? UpdatedBy { get; init; }

    public DateTime? UpdatedAt { get; init; }

    public string CreatedByText => this.GetCreatedByText();

    public string CreateDateText => this.GetCreateDateText();

    public string UpdatedByText => this.GetUpdatedByText();

    public string UpdatedAtText => this.GetUpdatedAtText();
}

/// <summary>客户编辑表单。</summary>
public class CustomerEditDto : IAuditFieldsDto
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public int Enable { get; set; } = 1;

    public string? CreatedBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string CreatedByText => this.GetCreatedByText();

    public string CreateDateText => this.GetCreateDateText();

    public string UpdatedByText => this.GetUpdatedByText();

    public string UpdatedAtText => this.GetUpdatedAtText();
}

/// <summary>料号条码规则列表行。</summary>
public class MaterialRuleListDto : IAuditFieldsDto
{
    public int RuleId { get; init; }

    public int CustomerId { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public string MaterialNo { get; init; } = string.Empty;

    public string SegmentSummary { get; init; } = string.Empty;

    public int BarcodeLength { get; init; }

    public string? CreatedBy { get; init; }

    public DateTime? CreateDate { get; init; }

    public string? UpdatedBy { get; init; }

    public DateTime? UpdatedAt { get; init; }

    public string CreatedByText => this.GetCreatedByText();

    public string CreateDateText => this.GetCreateDateText();

    public string UpdatedByText => this.GetUpdatedByText();

    public string UpdatedAtText => this.GetUpdatedAtText();
}

/// <summary>料号条码规则编辑表单（含规则段列表）。</summary>
public class MaterialRuleEditDto : IAuditFieldsDto
{
    public int RuleId { get; set; }

    public int CustomerId { get; set; }

    public string MaterialNo { get; set; } = string.Empty;

    public int BarcodeLength { get; set; }

    public IList<RuleSegmentEditDto> Segments { get; set; } = [];

    public string? CreatedBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string CreatedByText => this.GetCreatedByText();

    public string CreateDateText => this.GetCreateDateText();

    public string UpdatedByText => this.GetUpdatedByText();

    public string UpdatedAtText => this.GetUpdatedAtText();
}

/// <summary>单条规则段编辑项（类型、字面量/日期/流水号配置）。</summary>
public class RuleSegmentEditDto
{
    public int SegmentId { get; set; }

    public int SortOrder { get; set; }

    public string SegmentType { get; set; } = BarcodeSegmentTypes.Literal;

    public bool IncludeInResetKey { get; set; } = true;

    public string LiteralValue { get; set; } = string.Empty;

    public string DateFormat { get; set; } = DatePartFormats.Default;

    public int SerialRadix { get; set; } = 10;

    public int SerialDigits { get; set; } = 4;
}

/// <summary>条码拼接上下文（打印日期等）。</summary>
public class BarcodeBuildContext
{
    public DateTime PrintDate { get; set; } = DateTime.Today;
}

/// <summary>单条条码拼接结果。</summary>
public class BarcodeBuildResult
{
    public string Barcode { get; init; } = string.Empty;
}

/// <summary>生成前预览（流水号区间与样例条码）。</summary>
public class BarcodeGeneratePreviewDto
{
    public string ResetKey { get; init; } = string.Empty;

    public int SerialRadix { get; init; }

    public int SerialDigits { get; init; }

    public long NextSerialStart { get; init; }

    public long NextSerialEnd { get; init; }

    public string FirstSerialFormatted { get; init; } = string.Empty;

    public string LastSerialFormatted { get; init; } = string.Empty;

    public string FirstBarcodeSample { get; init; } = string.Empty;

    public string LastBarcodeSample { get; init; } = string.Empty;
}

/// <summary>条码批量生成请求。</summary>
public class BarcodeGenerateRequestDto
{
    public int RuleId { get; set; }

    public DateTime PrintDate { get; set; } = DateTime.Today;

    public int Quantity { get; set; }

    public string? CreatedBy { get; set; }
}

/// <summary>条码批量生成结果（生成单 Id 与明细列表）。</summary>
public class BarcodeGenerateResultDto
{
    public int GenerateRecordId { get; init; }

    public string GenerateNo { get; init; } = string.Empty;

    public IList<BarcodeRecordDto> Records { get; init; } = [];
}

/// <summary>单条条码明细。</summary>
public class BarcodeRecordDto
{
    public string Barcode { get; init; } = string.Empty;

    public long SerialValue { get; init; }
}

/// <summary>生成单列表行（含打印/补打状态）。</summary>
public class BarcodeGenerateRecordListDto
{
    public int GenerateRecordId { get; init; }

    public int RuleId { get; init; }

    public string GenerateNo { get; init; } = string.Empty;

    public string CustomerName { get; init; } = string.Empty;

    public string MaterialNo { get; init; } = string.Empty;

    public DateTime PrintDate { get; init; }

    public int Quantity { get; init; }

    public long SerialStart { get; init; }

    public long SerialEnd { get; init; }

    public string ResetKey { get; init; } = string.Empty;

    public string? CreatedBy { get; init; }

    public DateTime CreatedAt { get; init; }

    public int PrintStatus { get; init; }

    public string PrintStatusText => BarcodeOrderPrintStatus.GetText(PrintStatus);

    public DateTime? LastReprintedAt { get; init; }

    public string? LastReprintedBy { get; init; }

    public bool IsReprinted => PrintStatus == BarcodeOrderPrintStatus.Reprinted;

    public string LastReprintSummary =>
        LastReprintedAt == null
            ? string.Empty
            : $"最后补打：{LastReprintedAt:yyyy-MM-dd HH:mm}（{LastReprintedBy}）";

    public string SerialRangeText => $"{SerialStart} ~ {SerialEnd}";
}

/// <summary>生成单列表查询条件。</summary>
public class BarcodeGenerateRecordQueryDto
{
    public int? CustomerId { get; set; }

    public string? MaterialNo { get; set; }

    public string? GenerateNo { get; set; }

    /// <summary>生成时间下限（含当天 00:00:00）。</summary>
    public DateTime? CreatedFrom { get; set; }

    /// <summary>打印状态多选筛选；null 或空表示不限。</summary>
    public IReadOnlyList<int>? PrintStatuses { get; set; }
}

/// <summary>条码资料审核列表/详情共有字段。</summary>
public abstract class BarcodeQaReviewRowDto
{
    public int RuleId { get; init; }

    public int CustomerId { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public string MaterialNo { get; init; } = string.Empty;

    public int BarcodeLength { get; init; }

    public int QaStatus { get; init; }

    public string? AttachmentUploadedBy { get; init; }

    public DateTime? AttachmentUploadedAt { get; init; }

    public string? QaReviewedBy { get; init; }

    public DateTime? QaReviewedAt { get; init; }

    public string? QaReviewRemark { get; init; }

    public string QaStatusText => BarcodeQaStatus.GetText(QaStatus);

    public string AttachmentUploadedByText => AuditFieldDisplay.FormatUser(AttachmentUploadedBy);

    public string AttachmentUploadedAtText => AuditFieldDisplay.FormatDateTime(AttachmentUploadedAt);

    public string QaReviewedByText => AuditFieldDisplay.FormatUser(QaReviewedBy);

    public string QaReviewedAtText => AuditFieldDisplay.FormatDateTime(QaReviewedAt);
}

/// <summary>条码资料审核列表行（不含图片二进制）。</summary>
public class BarcodeQaReviewListDto : BarcodeQaReviewRowDto
{
    public bool HasDrawingImage { get; init; }

    public bool HasPrintSampleImage { get; init; }

    public string DrawingImageText => HasDrawingImage ? "已上传" : "未上传";

    public string PrintSampleImageText => HasPrintSampleImage ? "已上传" : "未上传";

    public string QaReviewRemarkText => string.IsNullOrWhiteSpace(QaReviewRemark) ? "-" : QaReviewRemark;
}

/// <summary>条码资料审核详情（含图片二进制，仅选中行加载）。</summary>
public class BarcodeQaReviewDetailDto : BarcodeQaReviewRowDto
{
    public byte[]? DrawingImage { get; init; }

    public byte[]? PrintSampleImage { get; init; }
}

/// <summary>条码资料审核保存附件请求。</summary>
public class BarcodeQaReviewSaveAttachmentsDto
{
    public int RuleId { get; set; }

    public byte[]? DrawingImage { get; set; }

    public byte[]? PrintSampleImage { get; set; }
}

/// <summary>条码资料审核驳回请求。</summary>
public class BarcodeQaReviewRejectDto
{
    public int RuleId { get; set; }

    public string RejectRemark { get; set; } = string.Empty;
}

/// <summary>规则段实体与编辑 DTO 互转。</summary>
public static class RuleSegmentConfigMapper
{
    public static RuleSegmentEditDto FromEntity(Entities.BarcodeRuleSegment entity)
    {
        var dto = new RuleSegmentEditDto
        {
            SegmentId = entity.SegmentId,
            SortOrder = entity.SortOrder,
            SegmentType = entity.SegmentType,
            IncludeInResetKey = entity.IncludeInResetKey == 1
        };

        try
        {
            RuleSegmentConfigHelper.ApplyFromConfigJson(dto, entity.ConfigJson);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"规则段配置解析失败: {entity.SegmentType}", ex);
        }

        return dto;
    }
}
