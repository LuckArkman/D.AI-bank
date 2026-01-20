namespace Fintech.Core.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Guid AccountId { get; private set; } // FK para a Conta Bancária
    public List<string> Roles { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User(string name, string email, string passwordHash, Guid accountId)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        AccountId = accountId;
        Roles = new List<string> { "Client" };
        CreatedAt = DateTime.UtcNow;
    }
}