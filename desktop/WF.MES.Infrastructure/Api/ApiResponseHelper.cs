using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Api;

public static class ApiResponseHelper
{
    public const string SessionReplacedCode = "session.replaced_by_other_device";

    public static T EnsureData<T>(ApiResultDto<T>? result, string fallbackMessage = "请求失败")
    {
        if (result is null)
        {
            throw new InvalidOperationException(fallbackMessage);
        }

        if (result.Code == 200)
        {
            return result.Data ?? throw new InvalidOperationException(fallbackMessage);
        }

        throw new InvalidOperationException(string.IsNullOrWhiteSpace(result.Message) ? fallbackMessage : result.Message);
    }

    public static bool IsSessionReplaced(ApiResultDto<object>? result) =>
        result?.Code == 401 && string.Equals(result.MessageCode, SessionReplacedCode, StringComparison.OrdinalIgnoreCase);
}
