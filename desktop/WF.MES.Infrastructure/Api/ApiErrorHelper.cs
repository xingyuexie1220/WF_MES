using System.Text.Json;
using Refit;
using WF.MES.Core.Exceptions;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Localization;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Api;

public static class ApiErrorHelper
{
    private const string DefaultApiBaseUrlHint = "http://localhost:5088";

    public static string ToUserMessage(Exception ex, ILocalizationService? localization = null, string? apiBaseUrl = null)
    {
        if (localization is not null && (ex is BusinessException || ex.InnerException is BusinessException))
        {
            return BusinessMessageResolver.Resolve(localization, ex);
        }

        if (TryParseApiResult(ex, out var messageCode, out var message) && localization is not null)
        {
            return ApiMessageResolver.Resolve(localization, messageCode, message);
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        var inner = ex;
        while (inner.InnerException is not null)
        {
            inner = inner.InnerException;
        }

        var innerMessage = inner.Message;
        if (inner is HttpRequestException or System.Net.Sockets.SocketException
            || innerMessage.Contains("积极拒绝", StringComparison.Ordinal)
            || innerMessage.Contains("actively refused", StringComparison.OrdinalIgnoreCase)
            || innerMessage.Contains("No connection could be made", StringComparison.OrdinalIgnoreCase))
        {
            var serviceTarget = Format(localization, "common.api.serviceTarget");
            var target = string.IsNullOrWhiteSpace(apiBaseUrl)
                ? serviceTarget
                : $"{serviceTarget} ({apiBaseUrl})";
            return Format(localization, "common.api.connectionRefused", target);
        }

        if (innerMessage.Contains("timed out", StringComparison.OrdinalIgnoreCase)
            || innerMessage.Contains("超时", StringComparison.Ordinal))
        {
            return Format(localization, "common.api.timeout");
        }

        if (innerMessage.Contains("ResponseEnded", StringComparison.OrdinalIgnoreCase)
            || innerMessage.Contains("response ended prematurely", StringComparison.OrdinalIgnoreCase)
            || innerMessage.Contains("连接被关闭", StringComparison.Ordinal))
        {
            var hint = string.IsNullOrWhiteSpace(apiBaseUrl) ? DefaultApiBaseUrlHint : apiBaseUrl;
            return Format(localization, "common.api.protocolMismatch", hint);
        }

        if (string.IsNullOrWhiteSpace(innerMessage))
        {
            return Format(localization, "common.api.loginRetry");
        }

        return innerMessage;
    }

    public static bool TryParseApiResult(Exception ex, out string? messageCode, out string? message)
    {
        messageCode = null;
        message = null;

        if (ex is not ApiException apiEx || string.IsNullOrWhiteSpace(apiEx.Content))
        {
            return false;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<ApiResultDto<object>>(apiEx.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (dto is null)
            {
                return false;
            }

            messageCode = dto.MessageCode;
            message = dto.Message;
            return !string.IsNullOrWhiteSpace(messageCode) || !string.IsNullOrWhiteSpace(message);
        }
        catch
        {
            return false;
        }
    }

    private static string Format(ILocalizationService? localization, string key, params object?[] args)
    {
        var template = localization?.T(key) ?? key;
        return args.Length > 0 ? string.Format(template, args) : template;
    }
}
