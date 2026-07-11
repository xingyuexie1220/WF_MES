using WF.MES.Core.Interfaces;

namespace WF.MES.Core.Extensions;

/// <summary>菜单内操作权限校验扩展。</summary>
public static class MenuActionAuthorizationExtensions
{
    public const string DefaultDeniedMessage = "无操作权限，请联系管理员分配相应权限后重新登录。";

    public static void EnsureAction(this IMenuActionAuthorization authorization, string actionCode)
    {
        if (!authorization.HasAction(actionCode))
        {
            throw new InvalidOperationException(DefaultDeniedMessage);
        }
    }
}
