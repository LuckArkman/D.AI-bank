namespace Fintech.Interfaces;

public interface IProductModule
{
    string ModuleId { get; }
    string Name { get; }
    bool IsActive { get; }
    Task InitializeAsync(Guid tenantId);
    Task DisableAsync(Guid tenantId);
}
