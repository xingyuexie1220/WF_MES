using System.Text.Json;
using Refit;
using WF.MES.Core.Exceptions;
using WF.MES.Core.Interfaces;
using WF.MES.Infrastructure.Localization;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Api;

/// <summary>API 异常 / messageCode → 用户可见文案。</summary>
public static class ApiErrorHelper
{
    private static readonly JsonSerializerOptions ParseOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>将 API messageCode 解析为当前语言文案；message 仅作 fallback。</summary>
    public static string ResolveMessage(
        ILocalizationService localization,
        string? messageCode,
        string? message = null,
        string fallbackKey = "common.requestFailed")
    {
        if (!string.IsNullOrWhiteSpace(messageCode))
        {
            var translated = localization.T(messageCode, "");
            if (!string.IsNullOrWhiteSpace(translated))
            {
                return translated;
            }
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        return localization.T(fallbackKey);
    }

    public static string ToUserMessage(Exception ex, ILocalizationService localization, string? apiBaseUrl = null)
    {
        if (ex is BusinessException || ex.InnerException is BusinessException)
        {
            return BusinessMessageResolver.Resolve(localization, ex);
        }

        if (TryParseApiResult(ex, out var messageCode, out var message))
        {
            return ResolveMessage(localization, messageCode, message);
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            return message!;
        }

        var inner = Unwrap(ex);
        var innerMessage = inner.Message;

        if (IsConnectionRefused(inner, innerMessage))
        {
            var serviceTarget = localization.T("common.api.serviceTarget");
            var target = string.IsNullOrWhiteSpace(apiBaseUrl)
                ? serviceTarget
                : $"{serviceTarget} ({apiBaseUrl})";
            return string.Format(localization.T("common.api.connectionRefused"), target);
        }

        if (IsTimeout(innerMessage))
        {
            return localization.T("common.api.timeout");
        }

        if (IsProtocolMismatch(innerMessage))
        {
            var hint = string.IsNullOrWhiteSpace(apiBaseUrl)
                ? localization.T("common.api.serviceTarget")
                : apiBaseUrl;
            return string.Format(localization.T("common.api.protocolMismatch"), hint);
        }

        return string.IsNullOrWhiteSpace(innerMessage)
            ? localization.T("common.api.loginRetry")
            : innerMessage;
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
            var dto = JsonSerializer.Deserialize<ApiResultDto<object>>(apiEx.Content, ParseOptions);
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

    private static Exception Unwrap(Exception ex)
    {
        while (ex.InnerException is not null)
        {
            ex = ex.InnerException;
        }

        return ex;
    }

    private static bool IsConnectionRefused(Exception inner, string innerMessage) =>
        inner is HttpRequestException or System.Net.Sockets.SocketException
        || innerMessage.Contains("积极拒绝", StringComparison.Ordinal)
        || innerMessage.Contains("actively refused", StringComparison.OrdinalIgnoreCase)
        || innerMessage.Contains("No connection could be made", StringComparison.OrdinalIgnoreCase);

    private static bool IsTimeout(string innerMessage) =>
        innerMessage.Contains("timed out", StringComparison.OrdinalIgnoreCase)
        || innerMessage.Contains("超时", StringComparison.Ordinal);

    private static bool IsProtocolMismatch(string innerMessage) =>
        innerMessage.Contains("ResponseEnded", StringComparison.OrdinalIgnoreCase)
        || innerMessage.Contains("response ended prematurely", StringComparison.OrdinalIgnoreCase)
        || innerMessage.Contains("连接被关闭", StringComparison.Ordinal);
}
