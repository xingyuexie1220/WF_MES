using FluentValidation;

namespace WF.MES.Infrastructure.Validation;

/// <summary>
/// 校验失败时抛出 <see cref="InvalidOperationException"/>，与现有 ViewModel Growl 提示保持一致。
/// </summary>
public static class ValidationExtensions
{
    public static async Task ValidateRequestAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (!result.IsValid)
        {
            throw new InvalidOperationException(result.Errors[0].ErrorMessage);
        }
    }

    public static void ValidateRequest<T>(this IValidator<T> validator, T instance)
    {
        var result = validator.Validate(instance);
        if (!result.IsValid)
        {
            throw new InvalidOperationException(result.Errors[0].ErrorMessage);
        }
    }
}
