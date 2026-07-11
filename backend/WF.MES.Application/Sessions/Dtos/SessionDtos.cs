using WF.MES.Shared.Enums;

namespace WF.MES.Application.Sessions.Dtos;

public class SessionDto
{
    public long UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? NickName { get; set; }

    public ClientType ClientType { get; set; }

    public string SessionId { get; set; } = string.Empty;

    public DateTime LoginTime { get; set; }

    public DateTime ExpireTime { get; set; }

    public bool IsActive { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }
}

public class SessionQueryRequest
{
    public int PageIndex { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? UserName { get; set; }

    public ClientType? ClientType { get; set; }

    public bool OnlyActive { get; set; } = true;
}

public class KickSessionRequest
{
    public long UserId { get; set; }

    public ClientType ClientType { get; set; }
}
