using FluentValidation;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Validation.Printing;

/// <summary><see cref="LabelPrintRequestDto"/> 校验。</summary>
public sealed class LabelPrintRequestValidator : AbstractValidator<LabelPrintRequestDto>
{
    public LabelPrintRequestValidator()
    {
        RuleFor(x => x.TemplatePath)
            .NotEmpty().WithMessage("标签模板路径不能为空");

        RuleFor(x => x.PrinterName)
            .NotEmpty().WithMessage("请选择打印机");

        RuleFor(x => x.Jobs)
            .NotEmpty().WithMessage("没有可打印的标签");
    }
}
