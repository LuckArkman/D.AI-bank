namespace Fintech.Interfaces;

public interface IAdminService
{
    Task<SystemStatsDto> GetSystemStatsAsync();
    Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync();
}

public record SystemStatsDto(int TotalAccounts, decimal TotalBalance, int PendingOutboxMessages);
public record AuditLogDto(Guid Id, string Action, string User, DateTime Timestamp);
