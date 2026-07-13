using FluentValidation;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Validation.Barcode;

/// <summary><see cref="CustomerEditDto"/> 校验。</summary>
public sealed class CustomerEditValidator : AbstractValidator<CustomerEditDto>
{
    public CustomerEditValidator(ILocalizationService localization)
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage(_ => localization.T("val.customerNameRequired"))
            .MaximumLength(100).WithMessage(_ => localization.T("val.customerNameTooLong"));

        RuleFor(x => x.Enable)
            .Must(v => v is 0 or 1)
            .WithMessage(_ => localization.T("val.enableInvalid"));
    }
}
