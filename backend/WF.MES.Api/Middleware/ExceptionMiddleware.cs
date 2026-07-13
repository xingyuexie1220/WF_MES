using System.Net;
using System.Text.Json;
using WF.MES.Application.Common;
using WF.MES.Application.Logs;
using WF.MES.Application.Logs.Dtos;
using WF.MES.Shared.Common;
using WF.MES.Shared.Constants;
using WF.MES.Shared.Exceptions;

namespace WF.MES.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IServiceScopeFactory scopeFactory)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException ex)
        {
            logger.LogWarning(ex, "Business exception: {Message}", ex.Message);
            await WriteResponseAsync(context, ex.Code, ex.Message, ex.MessageCode, ex.MessageArgs);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteExceptionLogAsync(context, ex);
            await WriteResponseAsync(context, (int)HttpStatusCode.InternalServerError, string.Empty, WfMessageCodes.CommonInternalError);
        }
    }

    private async Task WriteExceptionLogAsync(HttpContext context, Exception ex)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
            var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();

            string? requestParam = null;
            if (context.Request.ContentLength > 0 && context.Request.ContentLength <= 4096)
            {
                context.Request.EnableBuffering();
                context.Request.Body.Position = 0;
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                requestParam = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
                if (requestParam.Length > 4000)
                {
                    requestParam = requestParam[..4000];
                }
            }

            var path = context.Request.Path.Value ?? string.Empty;
            await logService.WriteExceptionLogAsync(new ExceptionLogDto
            {
                Module = path.Contains("/system/", StringComparison.OrdinalIgnoreCase) ? "system" : "api",
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                RequestUrl = path,
                RequestMethod = context.Request.Method,
                RequestParam = requestParam,
                OperIp = context.Connection.RemoteIpAddress?.ToString(),
                OperUserId = currentUser.UserId,
                OperUserName = currentUser.UserName
            });
        }
        catch (Exception logEx)
        {
            logger.LogWarning(logEx, "Failed to write exception log");
        }
    }

    private static async Task WriteResponseAsync(HttpContext context, int code, string messageOrCode, string? messageCode = null, object? messageArgs = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = code >= 500 ? StatusCodes.Status500InternalServerError : StatusCodes.Status200OK;

        var result = !string.IsNullOrWhiteSpace(messageCode)
            ? ApiResult.FailByCode(messageCode, code, messageArgs)
            : ApiResult.Fail(messageOrCode, code, null, messageArgs);
        await context.Response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
