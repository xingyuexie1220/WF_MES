using SqlSugar;
using WF.MES.Shared.Enums;

namespace WF.MES.Domain.Entities;

[SugarTable("Sys_Refresh_Token")]
public class SysRefreshToken
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    public long UserId { get; set; }

    public ClientType ClientType { get; set; } = ClientType.Web;

    public long? FactoryId { get; set; }

    public string? SessionId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpireTime { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public bool IsRevoked { get; set; }
}
