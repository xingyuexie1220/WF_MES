using FluentValidation;
using WF.MES.Core.Interfaces;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Validation.System;

/// <summary><see cref="PasswordChangeDto"/> 校验。</summary>
public sealed class PasswordChangeValidator : AbstractValidator<PasswordChangeDto>
{
    public PasswordChangeValidator(ILocalizationService localization)
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(_ => localization.T("mobile.password.newRequired"))
            .MinimumLength(6).WithMessage(_ => localization.T("mobile.password.tooShort"));

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword)
            .When(x => !string.IsNullOrEmpty(x.ConfirmPassword))
            .WithMessage(_ => localization.T("mobile.password.mismatch"));

        RuleFor(x => x)
            .Must(dto => dto.NewPassword != dto.CurrentPassword)
            .When(x => !string.IsNullOrEmpty(x.CurrentPassword))
            .WithMessage(_ => localization.T("mobile.password.sameAsOld"));
    }
}
