namespace Fintech.Interfaces;

public interface ITransactionManager
{
    Task<IUnitOfWork> BeginTransactionAsync();
}