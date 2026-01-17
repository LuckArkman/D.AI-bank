namespace Fintech.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Guid AccountId { get; private set; } // Link 1:1 com a Conta Bancária
    public string Role { get; private set; } // "CLIENT", "ADMIN"

    public User(string email, string passwordHash, Guid accountId, string role = "CLIENT")
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        AccountId = accountId;
        Role = role;
    }
}