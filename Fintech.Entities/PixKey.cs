namespace Fintech.Entities;

public class PixKey
{
    public string Key { get; private set; } // PK
    public string Type { get; private set; } // CPF, EMAIL, RANDOM
    public Guid AccountId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public PixKey(string key, string type, Guid accountId)
    {
        Key = key;
        Type = type;
        AccountId = accountId;
        CreatedAt = DateTime.UtcNow;
    }
}