namespace Fintech.Core.Interfaces;

public interface IMultiTenant
{
    Guid TenantId { get; }
}
