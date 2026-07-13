using WF.MES.Core.Exceptions;
using WF.MES.Core.Interfaces;

namespace WF.MES.Core.Extensions;

/// <summary>菜单内操作权限校验扩展。</summary>
public static class MenuActionAuthorizationExtensions
{
    public const string DefaultDeniedMessageCode = "err.actionDenied";

    public static void EnsureAction(this IMenuActionAuthorization authorization, string actionCode)
    {
        if (!authorization.HasAction(actionCode))
        {
            throw new BusinessException(DefaultDeniedMessageCode);
        }
    }
}
