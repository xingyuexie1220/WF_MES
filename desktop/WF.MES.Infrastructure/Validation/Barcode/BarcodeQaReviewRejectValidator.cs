using FluentValidation;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Validation.Barcode;

/// <summary>条码资料审核驳回校验。</summary>
public class BarcodeQaReviewRejectValidator : AbstractValidator<BarcodeQaReviewRejectDto>
{
    public BarcodeQaReviewRejectValidator(ILocalizationService localization)
    {
        RuleFor(x => x.RuleId)
            .GreaterThan(0)
            .WithMessage(_ => localization.T("val.ruleIdRequired"));
        RuleFor(x => x.RejectRemark)
            .NotEmpty().WithMessage(_ => localization.T("val.rejectRemarkRequired"))
            .MaximumLength(500).WithMessage(_ => localization.T("val.rejectRemarkTooLong"));
    }
}
