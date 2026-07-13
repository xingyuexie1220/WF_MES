using WF.MES.Core.Constants;
using WF.MES.Core.Exceptions;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Entities;

namespace WF.MES.Infrastructure.Services.Barcode;

/// <summary>条码模块审计字段与 QA 状态流转（保存规则会 ResetQaApproval，上传双图 → 待审核）。</summary>
internal static class BarcodeAuditHelper
{
    public static string? GetCurrentOperator(ISessionService sessionService) => sessionService.CurrentOperatorName;

    public static long GetCurrentFactoryId(ISessionService sessionService) =>
        sessionService.CurrentUser?.FactoryId
        ?? throw new BusinessException("err.factoryRequired");

    public static void ApplyCreateAudit(BarcodeCustomer entity, string? operatorName)
    {
        var now = DateTime.Now;
        entity.CreatedBy = operatorName;
        entity.CreateDate = now;
        entity.UpdatedBy = operatorName;
        entity.UpdatedAt = now;
    }

    public static void ApplyUpdateAudit(BarcodeCustomer entity, string? operatorName)
    {
        entity.UpdatedBy = operatorName;
        entity.UpdatedAt = DateTime.Now;
    }

    public static void ApplyCreateAudit(BarcodeMaterialRule entity, string? operatorName)
    {
        var now = DateTime.Now;
        entity.CreatedBy = operatorName;
        entity.CreateDate = now;
        entity.UpdatedBy = operatorName;
        entity.UpdatedAt = now;
        entity.QaStatus = BarcodeQaStatus.PendingUpload;
    }

    public static void ApplyUpdateAudit(BarcodeMaterialRule entity, string? operatorName)
    {
        entity.UpdatedBy = operatorName;
        entity.UpdatedAt = DateTime.Now;
    }

    public static void ResetQaApproval(BarcodeMaterialRule entity)
    {
        entity.QaStatus = BarcodeQaStatus.PendingUpload;
        entity.AttachmentUploadedBy = null;
        entity.AttachmentUploadedAt = null;
        entity.QaReviewedBy = null;
        entity.QaReviewedAt = null;
        entity.QaReviewRemark = null;
        entity.DrawingImage = null;
        entity.PrintSampleImage = null;
    }

    public static void ApplyQaApproval(BarcodeMaterialRule entity, string? operatorName)
    {
        var now = DateTime.Now;
        entity.QaStatus = BarcodeQaStatus.Approved;
        entity.QaReviewedBy = operatorName;
        entity.QaReviewedAt = now;
        entity.QaReviewRemark = null;
        entity.UpdatedBy = operatorName;
        entity.UpdatedAt = now;
    }

    public static void ApplyQaRejection(BarcodeMaterialRule entity, string? operatorName, string reviewRemark)
    {
        var now = DateTime.Now;
        entity.QaStatus = BarcodeQaStatus.Rejected;
        entity.QaReviewedBy = operatorName;
        entity.QaReviewedAt = now;
        entity.QaReviewRemark = reviewRemark.Trim();
        entity.UpdatedBy = operatorName;
        entity.UpdatedAt = now;
    }

    public static void ApplyQaAttachmentsSaved(BarcodeMaterialRule entity, string? operatorName)
    {
        var now = DateTime.Now;
        entity.QaReviewedBy = null;
        entity.QaReviewedAt = null;
        entity.QaReviewRemark = null;
        entity.QaStatus = HasBothImages(entity)? BarcodeQaStatus.PendingReview : BarcodeQaStatus.PendingUpload;

        if (entity.QaStatus == BarcodeQaStatus.PendingReview)
        {
            entity.AttachmentUploadedBy = operatorName;
            entity.AttachmentUploadedAt = now;
        }

        entity.UpdatedBy = operatorName;
        entity.UpdatedAt = now;
    }

    private static bool HasBothImages(BarcodeMaterialRule entity) =>
        entity.DrawingImage is { Length: > 0 } && entity.PrintSampleImage is { Length: > 0 };
}
