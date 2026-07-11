using FluentValidation;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Validation.Barcode;

/// <summary>条码资料审核驳回校验。</summary>
public class BarcodeQaReviewRejectValidator : AbstractValidator<BarcodeQaReviewRejectDto>
{
    public BarcodeQaReviewRejectValidator()
    {
        RuleFor(x => x.RuleId).GreaterThan(0).WithMessage("请选择料号规则");
        RuleFor(x => x.RejectRemark)
            .NotEmpty().WithMessage("请填写驳回原因")
            .MaximumLength(500).WithMessage("驳回原因不能超过 500 字");
    }
}
