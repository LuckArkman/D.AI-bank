using Fintech.Core.Interfaces;
using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Exceptions;

namespace Fintech.Application.Commands;

public class RegisterPixKeyHandler
{
    private readonly IPixKeyRepository _pixRepo;
    private readonly ITransactionManager _txManager;
    private readonly ITenantProvider _tenantProvider;

    public RegisterPixKeyHandler(IPixKeyRepository pixRepo, ITransactionManager txManager, ITenantProvider tenantProvider)
    {
        _pixRepo = pixRepo;
        _txManager = txManager;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(Guid accountId, string key, string type)
    {
        // 1. Validação de Regra de Negócio
        if (await _pixRepo.ExistsAsync(key))
            throw new DomainException($"A chave Pix '{key}' já está em uso.");

        var validTypes = new[] { "CPF", "EMAIL", "RANDOM", "PHONE" };
        if (!validTypes.Contains(type.ToUpper()))
            throw new DomainException("Tipo de chave Pix inválido.");

        using var uow = await _txManager.BeginTransactionAsync();
        try
        {
            var tenantId = _tenantProvider.TenantId ?? throw new DomainException("TenantId não resolvido.");
            var pixKey = new PixKey(key, type.ToUpper(), tenantId, accountId);
            await _pixRepo.AddAsync(pixKey);
            await uow.CommitAsync();
        }
        catch
        {
            await uow.AbortAsync();
            throw;
        }
    }
}