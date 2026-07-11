using FluentValidation;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Validation.Barcode;

/// <summary><see cref="CustomerEditDto"/> 校验。</summary>
public sealed class CustomerEditValidator : AbstractValidator<CustomerEditDto>
{
    public CustomerEditValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("客户名称不能为空")
            .MaximumLength(100).WithMessage("客户名称不能超过 100 个字符");

        RuleFor(x => x.Enable)
            .Must(v => v is 0 or 1)
            .WithMessage("启用状态无效");
    }
}
