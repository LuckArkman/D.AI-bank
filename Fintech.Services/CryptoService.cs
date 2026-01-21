using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.ValueObjects;
using Fintech.Core.Interfaces;

namespace Fintech.Services;

public interface ICryptoService
{
    Task<List<CryptoAsset>> GetWalletAsync(Guid accountId);
    Task BuyCryptoAsync(Guid accountId, string symbol, decimal amountBrl);
    Task SellCryptoAsync(Guid accountId, string symbol, decimal amountAsset);
}

public class CryptoService : ICryptoService
{
    private readonly ICryptoRepository _cryptoRepo;
    private readonly IAccountRepository _accountRepo;
    private readonly ICurrencyExchangeService _exchangeService;
    private readonly ITenantProvider _tenantProvider;

    // Simulated Crypto Prices (in BRL)
    private readonly Dictionary<string, decimal> _prices = new()
    {
        ["BTC"] = 245000m,
        ["ETH"] = 12500m,
        ["SOL"] = 550m
    };

    public CryptoService(
        ICryptoRepository cryptoRepo,
        IAccountRepository accountRepo,
        ICurrencyExchangeService exchangeService,
        ITenantProvider tenantProvider)
    {
        _cryptoRepo = cryptoRepo;
        _accountRepo = accountRepo;
        _exchangeService = exchangeService;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<CryptoAsset>> GetWalletAsync(Guid accountId)
    {
        return await _cryptoRepo.GetByAccountIdAsync(accountId);
    }

    public async Task BuyCryptoAsync(Guid accountId, string symbol, decimal amountBrl)
    {
        var account = await _accountRepo.GetByIdAsync(accountId);

        // 1. Debit BRL from account
        account.Debit(Money.BRL(amountBrl));
        await _accountRepo.UpdateAsync(account);

        // 2. Calculate crypto amount
        var price = _prices.GetValueOrDefault(symbol, 0);
        if (price == 0) throw new Exception("Asset not supported");

        var cryptoAmount = amountBrl / price;

        // 3. Update or Create CryptoAsset
        var asset = await _cryptoRepo.GetByAccountAndSymbolAsync(accountId, symbol);
        if (asset == null)
        {
            asset = new CryptoAsset(accountId, _tenantProvider.TenantId!.Value, symbol, symbol, "0x" + Guid.NewGuid().ToString("N").Substring(0, 40));
            asset.Balance = cryptoAmount;
            await _cryptoRepo.AddAsync(asset);
        }
        else
        {
            asset.Balance += cryptoAmount;
            asset.LastUpdated = DateTime.UtcNow;
            await _cryptoRepo.UpdateAsync(asset);
        }
    }

    public async Task SellCryptoAsync(Guid accountId, string symbol, decimal amountAsset)
    {
        var asset = await _cryptoRepo.GetByAccountAndSymbolAsync(accountId, symbol);
        if (asset == null || asset.Balance < amountAsset) throw new Exception("Insufficient crypto balance");

        // 1. Calculate BRL value
        var price = _prices.GetValueOrDefault(symbol, 0);
        var brlValue = amountAsset * price;

        // 2. Debit Crypto
        asset.Balance -= amountAsset;
        asset.LastUpdated = DateTime.UtcNow;
        await _cryptoRepo.UpdateAsync(asset);

        // 3. Credit BRL to account
        var account = await _accountRepo.GetByIdAsync(accountId);
        account.Credit(Money.BRL(brlValue));
        await _accountRepo.UpdateAsync(account);
    }
}
