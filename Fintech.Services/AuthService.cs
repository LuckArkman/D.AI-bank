using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fintech.Commands;
using Fintech.Core.Entities;
using Fintech.Core.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Fintech.Interfaces;
using Fintech.Records;
using Microsoft.Extensions.Configuration;

namespace Fintech.Services;

public class AuthService
{
    private readonly IUserRepository _userRepo;
    private readonly ICreateAccountHandler _createAccountHandler;
    private readonly ITransactionManager _txManager;
    private readonly IConfiguration _config;
    private readonly ITenantProvider _tenantProvider;

    public AuthService(
        IUserRepository userRepo,
        ICreateAccountHandler createAccountHandler,
        ITransactionManager txManager,
        IConfiguration config,
        ITenantProvider tenantProvider)
    {
        _userRepo = userRepo;
        _createAccountHandler = createAccountHandler;
        _txManager = txManager;
        _config = config;
        _tenantProvider = tenantProvider;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            if (await _userRepo.ExistsByEmailAsync(request.Email))
                throw new Exception("Email já está em uso.");

            var tenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId não resolvido.");
            var accountId = await _createAccountHandler.Handle(request.InitialDeposit, request.ProfileType, request.CurrencyCode);

            var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);


            // Agora User tem Name no construtor
            var user = new User(request.Name, request.Email, hash, accountId, tenantId);

            await _userRepo.AddAsync(user);

            await uow.CommitAsync();

            var token = GenerateJwt(user);
            return new AuthResponse(token, user.Name, user.Email, user.AccountId);
        }
        catch
        {
            await uow.AbortAsync();
            throw;
        }
    }


    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new Exception("Email ou senha inválidos.");

        var token = GenerateJwt(user);
        return new AuthResponse(token, user.Name, user.Email, user.AccountId);
    }

    private string GenerateJwt(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim("AccountId", user.AccountId.ToString()),
            new Claim("TenantId", user.TenantId.ToString()),
            new Claim(ClaimTypes.Role, "Client")
        };

        var secret = _config["Jwt:Secret"] ?? "Segredo_Super_Secreto_Para_Dev_Local_123!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


        var token = new JwtSecurityToken(
            issuer: "Fintech.Api",
            audience: "Fintech.App",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}