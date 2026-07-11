namespace WF.MES.Models.Dtos;

public class ApiResultDto<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MessageCode { get; set; }
    public T? Data { get; set; }
}

public class LoginRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    /// <summary>1=Web 2=Mobile 3=Desktop</summary>
    public int ClientType { get; set; } = 3;
    public string? Locale { get; set; } = "zh-CN";
    public long? FactoryId { get; set; }
}

public class SelectFactoryRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public long FactoryId { get; set; }
    public int ClientType { get; set; } = 3;
    public string? Locale { get; set; } = "zh-CN";
}

public class FactorySummaryDto
{
    public long Id { get; set; }
    public string FactoryCode { get; set; } = string.Empty;
    public string FactoryName { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public bool NeedSelectFactory { get; set; }
    public List<FactorySummaryDto> Factories { get; set; } = [];
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UserInfoDto UserInfo { get; set; } = new();
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

public class SwitchFactoryRequestDto
{
    public long FactoryId { get; set; }
}

public class ChangePasswordRequestDto
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ClientMenuDto
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public int MenuType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? I18nKey { get; set; }
    public string? Icon { get; set; }
    public string? Path { get; set; }
    public string? Component { get; set; }
    public string? Permission { get; set; }
    public bool Visible { get; set; } = true;
    public int Sort { get; set; }
    public List<ClientMenuDto> Children { get; set; } = [];
}

public class LoginResultDto
{
    public bool Success { get; set; }
    public bool NeedSelectFactory { get; set; }
    public List<FactorySummaryDto> Factories { get; set; } = [];
    public string? ErrorMessage { get; set; }
    public UserInfoDto? User { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
