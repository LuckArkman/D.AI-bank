using Fintech.Core.Interfaces;

namespace Fintech.Entities;

public class CryptoAsset : IMultiTenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid AccountId { get; set; }
    public string Symbol { get; set; } // BTC, ETH, SOL
    public string Name { get; set; }
    public decimal Balance { get; set; }
    public string WalletAddress { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public CryptoAsset() { }

    public CryptoAsset(Guid accountId, Guid tenantId, string symbol, string name, string walletAddress)
    {
        AccountId = accountId;
        TenantId = tenantId;
        Symbol = symbol;
        Name = name;
        WalletAddress = walletAddress;
        Balance = 0;
    }
}
