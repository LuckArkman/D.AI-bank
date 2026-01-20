using Fintech.Enums;

namespace Fintech.Entities;

public class AccountCard
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string LastFourDigits { get; private set; }
    public string Brand { get; private set; } // Visa, Mastercard, etc.
    public string HolderName { get; private set; }
    public CardType Type { get; private set; }
    public CardStatus Status { get; private set; }
    public bool IsVirtual { get; private set; }
    public decimal CreditLimit { get; private set; }
    public decimal AvailableCredit { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public AccountCard(Guid accountId, string lastFourDigits, string brand, string holderName, CardType type, bool isVirtual, decimal creditLimit = 0)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        LastFourDigits = lastFourDigits;
        Brand = brand;
        HolderName = holderName;
        Type = type;
        IsVirtual = isVirtual;
        Status = CardStatus.Active;
        CreditLimit = creditLimit;
        AvailableCredit = creditLimit;
        ExpiryDate = DateTime.UtcNow.AddYears(5);
        CreatedAt = DateTime.UtcNow;
    }

    public void Block() => Status = CardStatus.Blocked;
    public void Unblock() => Status = CardStatus.Active;
}
