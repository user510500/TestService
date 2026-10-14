namespace TestService.Application.Interfaces;

/// <summary>密码哈希服务接口（BCrypt 实现）。</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hashed);
}
