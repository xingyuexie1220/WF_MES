using WF.MES.Core.Exceptions;
using WF.MES.Models.Dtos;

namespace WF.MES.Infrastructure.Api;

public static class ApiResponseHelper
{
    public const string SessionReplacedCode = "session.replaced_by_other_device";

    public static T EnsureData<T>(ApiResultDto<T>? result, string fallbackMessageCode)
    {
        if (result is null)
        {
            throw new BusinessException(fallbackMessageCode);
        }

        if (result.Code == 200)
        {
            return result.Data ?? throw new BusinessException(fallbackMessageCode);
        }

        var messageCode = string.IsNullOrWhiteSpace(result.MessageCode)
            ? fallbackMessageCode
            : result.MessageCode;

        throw new BusinessException(messageCode);
    }

    public static bool IsSessionReplaced<T>(ApiResultDto<T>? result) =>
        result?.Code == 401 && string.Equals(result.MessageCode, SessionReplacedCode, StringComparison.OrdinalIgnoreCase);
}
