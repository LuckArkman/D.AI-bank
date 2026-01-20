namespace Fintech.Interfaces;

public interface ITenantProvider
{
    Guid? TenantId { get; }
}
