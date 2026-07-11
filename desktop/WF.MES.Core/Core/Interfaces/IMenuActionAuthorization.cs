namespace WF.MES.Core.Interfaces;

/// <summary>基于当前 Session 的菜单内操作权限校验。</summary>
public interface IMenuActionAuthorization
{
    bool HasAction(string actionCode);
}
