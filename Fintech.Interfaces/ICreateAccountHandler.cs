namespace Fintech.Interfaces;

public interface ICreateAccountHandler
{
    Task<Guid> Handle(decimal initialBalance);
}