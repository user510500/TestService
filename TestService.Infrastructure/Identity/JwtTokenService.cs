using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TestService.Application.Interfaces;
using TestService.Infrastructure.Options;
using JwtPayload = TestService.Application.Interfaces.JwtPayload;

namespace TestService.Infrastructure.Identity;

/// <summary>JWT 令牌服务实现（HS256 对称加密）。</summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(Guid userId, string username, IEnumerable<string> roles, IEnumerable<string>? permissions = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username)
        };
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));
        if (permissions is not null)
        {
            foreach (var p in permissions) claims.Add(new Claim("permission", p));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public JwtPayload? ParseToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var jwt = handler.ReadJwtToken(token);
            var userId = Guid.Parse(jwt.Subject);
            var username = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty;
            var roles = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            var permissions = jwt.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
            return new JwtPayload
            {
                UserId = userId,
                Username = username,
                Roles = roles,
                Permissions = permissions,
                ExpiresAt = jwt.ValidTo
            };
        }
        catch
        {
            return null;
        }
    }
}
