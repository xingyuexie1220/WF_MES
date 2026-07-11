using WF.MES.Application.Factories.Dtos;
using WF.MES.Shared.Enums;

namespace WF.MES.Application.Auth.Dtos;

public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public ClientType ClientType { get; set; } = ClientType.Web;
    public string? Locale { get; set; }
    public long? FactoryId { get; set; }
}

public class LoginResponse
{
    public bool NeedSelectFactory { get; set; }
    public List<FactorySummaryDto> Factories { get; set; } = [];
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UserInfoDto UserInfo { get; set; } = new();
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class UserInfoDto
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? NickName { get; set; }
    public long DeptId { get; set; }
    public string? DeptName { get; set; }
    public long? FactoryId { get; set; }
    public string? FactoryCode { get; set; }
    public string? FactoryName { get; set; }
    public bool MustChangePassword { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
    public List<FactorySummaryDto> AccessibleFactories { get; set; } = [];
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
