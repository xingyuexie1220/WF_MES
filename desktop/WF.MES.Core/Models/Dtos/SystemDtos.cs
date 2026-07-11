namespace WF.MES.Models.Dtos;

/// <summary>修改密码请求（改密对话框与 Validator 共用）。</summary>
public sealed class PasswordChangeDto
{
    public string NewPassword { get; init; } = string.Empty;

    /// <summary>确认新密码；仅改密对话框传入，Service 层可省略。</summary>
    public string ConfirmPassword { get; init; } = string.Empty;

    /// <summary>当前密码；仅改密对话框传入，用于禁止与旧密码相同。</summary>
    public string CurrentPassword { get; init; } = string.Empty;
}
