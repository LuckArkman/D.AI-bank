using Fintech.Entities;

namespace Fintech.Interfaces;

public interface ICryptoRepository
{
    Task<CryptoAsset?> GetByAccountAndSymbolAsync(Guid accountId, string symbol);
    Task<List<CryptoAsset>> GetByAccountIdAsync(Guid accountId);
    Task AddAsync(CryptoAsset asset);
    Task UpdateAsync(CryptoAsset asset);
}
