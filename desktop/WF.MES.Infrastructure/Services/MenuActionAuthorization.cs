using WF.MES.Core.Interfaces;

namespace WF.MES.Infrastructure.Services;

/// <summary>按钮权限：检查 Session 中的 API Permission 集合。</summary>
public sealed class MenuActionAuthorization(ISessionService sessionService) : IMenuActionAuthorization
{
    public bool HasAction(string actionCode)
    {
        if (string.IsNullOrWhiteSpace(actionCode) || sessionService.CurrentUser is null)
        {
            return false;
        }

        return sessionService.PermittedActionCodes.Contains(actionCode);
    }
}
