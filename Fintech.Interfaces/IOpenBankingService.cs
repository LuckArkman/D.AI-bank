using Fintech.DTOs;

namespace Fintech.Interfaces;

public interface IOpenBankingService
{
    Task<IEnumerable<AccountDto>> GetAccountsAsync();
    Task<BalanceDto> GetBalanceAsync(Guid accountId);
    Task<IEnumerable<TransactionDto>> GetTransactionsAsync(Guid accountId);
}

public record AccountDto(Guid Id, string Type, string Currency);
public record BalanceDto(decimal Amount, string Currency);
public record TransactionDto(Guid Id, string Type, decimal Amount, DateTime Timestamp);
