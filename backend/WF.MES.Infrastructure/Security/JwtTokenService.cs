using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WF.MES.Domain.Entities;
using WF.MES.Infrastructure.Options;
using WF.MES.Shared.Constants;
using WF.MES.Shared.Enums;

namespace WF.MES.Infrastructure.Security;

public class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public (string AccessToken, DateTime ExpireAt) CreateAccessToken(
        SystemUser user,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        ClientType clientType,
        string sessionId,
        long factoryId,
        string? factoryCode = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(WfClaimTypes.UserId, user.Id.ToString()),
            new(WfClaimTypes.UserName, user.UserName),
            new(WfClaimTypes.DeptId, user.DeptId.ToString()),
            new(WfClaimTypes.ClientType, ((int)clientType).ToString()),
            new(WfClaimTypes.SessionId, sessionId),
            new(WfClaimTypes.FactoryId, factoryId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        if (!string.IsNullOrWhiteSpace(factoryCode))
        {
            claims.Add(new Claim("wf:factory_code", factoryCode));
        }

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim(WfClaimTypes.Permission, p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expireAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpireMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expireAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expireAt);
    }

    public string CreateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
