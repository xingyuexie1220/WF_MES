using FluentValidation;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Validation.Barcode;

/// <summary><see cref="BarcodeGenerateRequestDto"/> 校验。</summary>
public sealed class BarcodeGenerateRequestValidator : AbstractValidator<BarcodeGenerateRequestDto>
{
    public BarcodeGenerateRequestValidator(ILocalizationService localization)
    {
        RuleFor(x => x.RuleId)
            .GreaterThan(0)
            .WithMessage(_ => localization.T("val.ruleRequired"));

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage(_ => localization.T("val.quantityPositive"))
            .LessThanOrEqualTo(BarcodeGenerateLimits.MaxQuantityPerBatch)
            .WithMessage(_ => string.Format(
                localization.T("val.quantityMax"),
                BarcodeGenerateLimits.MaxQuantityPerBatch));
    }
}
