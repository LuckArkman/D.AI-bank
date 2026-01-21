using Fintech.Enums;

namespace Fintech.Interfaces;

public interface ICreateAccountHandler
{
    Task<Guid> Handle(decimal initialBalance, AccountProfileType profileType = AccountProfileType.StandardIndividual);
    Task<Guid> Handle(decimal initialBalance, Guid tenantId, AccountProfileType profileType = AccountProfileType.StandardIndividual);
}
