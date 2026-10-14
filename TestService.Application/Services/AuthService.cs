using Microsoft.Extensions.Logging;
using TestService.Application.Dtos;
using TestService.Application.Interfaces;
using TestService.Domain.Entities;
using TestService.Domain.Exceptions;
using TestService.Domain.Interfaces;

namespace TestService.Application.Services;

/// <summary>
/// 认证服务实现：登录、注册、当前用户查询。
/// </summary>
public class AuthService : IAuthService
{
    private readonly IRepository<SysUser> _userRepo;
    private readonly IRepository<SysRole> _roleRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IRepository<SysUser> userRepo,
        IRepository<SysRole> roleRepo,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepo.FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsDeleted, cancellationToken);
        if (user is null)
        {
            throw new DomainException("用户名或密码错误", 401);
        }
        if (user.Status == 0)
        {
            throw new DomainException("账号已被禁用", 401);
        }
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new DomainException("用户名或密码错误", 401);
        }

        // 加载角色
        var roles = await _roleRepo.GetListAsync(r => user.UserRoles.Select(ur => ur.RoleId).Contains(r.Id), cancellationToken);
        var roleNames = roles.Select(r => r.Code).ToList();

        user.LastLoginTime = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(user.Id, user.Username, roleNames);
        return new LoginResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = 3600 * 8,
            User = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                RealName = user.RealName,
                Email = user.Email,
                Phone = user.Phone,
                Roles = roleNames
            }
        };
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3)
            throw new DomainException("用户名至少 3 个字符");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new DomainException("密码至少 6 位");

        var exists = await _userRepo.ExistsAsync(u => u.Username == request.Username, cancellationToken);
        if (exists)
            throw new DomainException("用户名已存在");

        var user = new SysUser
        {
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Salt = string.Empty,
            RealName = request.RealName,
            Email = request.Email,
            Phone = request.Phone,
            Status = 1
        };
        await _userRepo.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("用户 {Username} 注册成功", user.Username);
    }

    public async Task<UserInfoDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, cancellationToken)
            ?? throw new DomainException("用户不存在", 404);
        var roles = await _roleRepo.GetListAsync(r => user.UserRoles.Select(ur => ur.RoleId).Contains(r.Id), cancellationToken);
        return new UserInfoDto
        {
            Id = user.Id,
            Username = user.Username,
            RealName = user.RealName,
            Email = user.Email,
            Phone = user.Phone,
            Roles = roles.Select(r => r.Code).ToList()
        };
    }
}
