using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WF.MES.Shared.Common;
using WF.MES.Shared.Constants;

namespace WF.MES.Api.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var argumentType = argument.GetType();
            if (argumentType.IsPrimitive || argumentType == typeof(string) || argumentType == typeof(CancellationToken))
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument));
            if (result.IsValid)
            {
                continue;
            }

            var message = result.Errors.FirstOrDefault()?.ErrorMessage ?? "请求参数无效";
            context.Result = new OkObjectResult(ApiResult.Fail(message, 400, WfMessageCodes.ValidationFailed));
            return;
        }

        await next();
    }
}
