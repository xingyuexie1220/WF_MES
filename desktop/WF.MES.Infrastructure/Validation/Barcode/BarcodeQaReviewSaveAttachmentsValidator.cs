using FluentValidation;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Validation.Barcode;

/// <summary>条码资料审核保存附件校验。</summary>
public class BarcodeQaReviewSaveAttachmentsValidator : AbstractValidator<BarcodeQaReviewSaveAttachmentsDto>
{
    public BarcodeQaReviewSaveAttachmentsValidator()
    {
        RuleFor(x => x.RuleId).GreaterThan(0).WithMessage("请选择料号规则");
    }
}
