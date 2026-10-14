using TestService.Application.Interfaces;

namespace TestService.Infrastructure.Identity;

/// <summary>基于 BCrypt 的密码哈希服务。</summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hashed)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashed);
        }
        catch
        {
            return false;
        }
    }
}
