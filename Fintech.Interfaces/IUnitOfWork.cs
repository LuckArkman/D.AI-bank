namespace Fintech.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task AbortAsync(CancellationToken ct = default);
}