using FluentValidation;
using WF.MES.Core.Constants;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Validation.Barcode;

/// <summary><see cref="RuleSegmentEditDto"/> 校验。</summary>
public sealed class RuleSegmentEditValidator : AbstractValidator<RuleSegmentEditDto>
{
    public RuleSegmentEditValidator(ILocalizationService localization)
    {
        RuleFor(x => x.SegmentType)
            .Must(BarcodeSegmentTypes.IsValid)
            .WithMessage(_ => localization.T("val.segmentTypeInvalid"));

        When(x => x.SegmentType == BarcodeSegmentTypes.Literal, () =>
        {
            RuleFor(x => x.LiteralValue)
                .NotEmpty().WithMessage(_ => localization.T("val.literalRequired"));

            RuleFor(x => x.LiteralValue)
                .Must(v => v == v.Trim())
                .WithMessage(_ => localization.T("val.literalTrim"));
        });

        When(x => x.SegmentType == BarcodeSegmentTypes.Date, () =>
        {
            RuleFor(x => x.DateFormat)
                .Must(DatePartFormats.IsValid)
                .WithMessage(x => string.Format(
                    localization.T("val.dateFormatInvalid"),
                    x.DateFormat));
        });

        When(x => x.SegmentType == BarcodeSegmentTypes.Serial, () =>
        {
            RuleFor(x => x.SerialRadix)
                .Must(SerialRadixDefinitions.IsSupported)
                .WithMessage(_ => localization.T("val.serialRadixInvalid"));

            RuleFor(x => x.SerialDigits)
                .InclusiveBetween(1, 20)
                .WithMessage(_ => localization.T("val.serialDigitsInvalid"));
        });
    }
}
