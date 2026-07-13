using FluentValidation;
using WF.MES.Core.Exceptions;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Localization;
using WF.MES.Models.Dtos;
using WF.MES.Models.Entities;

namespace WF.MES.Infrastructure.Validation.Barcode;

/// <summary><see cref="MaterialRuleEditDto"/> 校验（含规则段与条码长度）。</summary>
public sealed class MaterialRuleEditValidator : AbstractValidator<MaterialRuleEditDto>
{
    public MaterialRuleEditValidator(IBarcodeBuilder barcodeBuilder, ILocalizationService localization)
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage(_ => localization.T("val.customerRequired"));

        RuleFor(x => x.MaterialNo)
            .NotEmpty().WithMessage(_ => localization.T("val.materialNoRequired"))
            .MaximumLength(50).WithMessage(_ => localization.T("val.materialNoTooLong"));

        RuleFor(x => x.BarcodeLength)
            .GreaterThan(0)
            .WithMessage(_ => localization.T("val.barcodeLengthRequired"));

        RuleFor(x => x.Segments)
            .NotEmpty()
            .WithMessage(_ => localization.T("val.segmentRequired"));

        RuleForEach(x => x.Segments)
            .SetValidator(new RuleSegmentEditValidator(localization));

        RuleFor(x => x)
            .Custom((dto, context) =>
            {
                if (dto.Segments.Count == 0)
                {
                    return;
                }

                var entities = dto.Segments
                    .Select(s => new BarcodeRuleSegment
                    {
                        SegmentType = s.SegmentType,
                        SortOrder = s.SortOrder,
                        ConfigJson = RuleSegmentConfigHelper.ToConfigJson(s),
                        IncludeInResetKey = s.IncludeInResetKey ? 1 : 0
                    })
                    .ToList();

                try
                {
                    barcodeBuilder.ValidateSegments(entities);
                }
                catch (BusinessException ex)
                {
                    context.AddFailure(BusinessMessageResolver.Resolve(localization, ex));
                    return;
                }

                var calculatedLength = barcodeBuilder.CalculateBarcodeLength(entities);
                if (dto.BarcodeLength != calculatedLength)
                {
                    context.AddFailure(string.Format(
                        localization.T("val.barcodeLengthMismatch"),
                        dto.BarcodeLength,
                        calculatedLength));
                }
            });
    }
}
