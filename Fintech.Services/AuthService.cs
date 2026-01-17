using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fintech.Entities;
using Fintech.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Fintech.Interfaces;

namespace Fintech.Services;

public class AuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly ICreateAccountHandler _accountHandler; // Reutiliza lógica de criar conta
    private readonly IConfiguration _config;

    public async Task<string> RegisterAsync(string email, string password)
    {
        // 1. Verifica duplicidade
        if (await _users.Find(x => x.Email == email).AnyAsync())
            throw new DomainException("Email já cadastrado.");

        // 2. Cria a Conta Bancária (Lógica do Sprint Anterior)
        // Isso garante que todo User tenha uma AccountId válida.
        var accountId = await _accountHandler.Handle(initialBalance: 0);

        // 3. Hash da Senha (Nunca salve em texto plano!)
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // 4. Salva User
        var user = new User(email, hash, accountId);
        await _users.InsertOneAsync(user);

        return GenerateJwt(user);
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var user = await _users.Find(x => x.Email == email).FirstOrDefaultAsync();
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new DomainException("Credenciais inválidas.");

        return GenerateJwt(user);
    }

    private string GenerateJwt(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("AccountId", user.AccountId.ToString()), // O Claim vital!
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}