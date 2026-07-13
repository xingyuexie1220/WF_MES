using FluentValidation;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Validation.Printing;

/// <summary><see cref="LabelPrintRequestDto"/> 校验。</summary>
public sealed class LabelPrintRequestValidator : AbstractValidator<LabelPrintRequestDto>
{
    public LabelPrintRequestValidator(ILocalizationService localization)
    {
        RuleFor(x => x.TemplatePath)
            .NotEmpty().WithMessage(_ => localization.T("val.templatePathRequired"));

        RuleFor(x => x.PrinterName)
            .NotEmpty().WithMessage(_ => localization.T("val.printerRequired"));

        RuleFor(x => x.Jobs)
            .NotEmpty().WithMessage(_ => localization.T("val.labelsRequired"));
    }
}
