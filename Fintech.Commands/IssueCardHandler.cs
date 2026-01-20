using Fintech.Entities;
using Fintech.Enums;
using Fintech.Interfaces;
using Fintech.Core.Interfaces;

namespace Fintech.Commands;

public class IssueCardHandler
{
    private readonly ICardRepository _cardRepo;
    private readonly IAccountRepository _accountRepo;
    private readonly ITenantProvider _tenantProvider;
    private readonly ITenantRepository _tenantRepo;

    public IssueCardHandler(ICardRepository cardRepo, IAccountRepository accountRepo, ITenantProvider tenantProvider, ITenantRepository tenantRepo)
    {
        _cardRepo = cardRepo;
        _accountRepo = accountRepo;
        _tenantProvider = tenantProvider;
        _tenantRepo = tenantRepo;
    }

    public async Task<Guid> Handle(Guid accountId, string brand, CardType type, bool isVirtual, decimal creditLimit = 0)
    {
        var tenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId não resolvido.");
        var tenant = await _tenantRepo.GetByIdAsync(tenantId);

        if (tenant == null || (!tenant.ActiveModes.Contains(BusinessMode.DigitalBank) && !tenant.ActiveModes.Contains(BusinessMode.PaymentInstitution)))
        {
            throw new Exception("Módulo de Cartões não está ativo para este Perfil Institucional.");
        }

        var account = await _accountRepo.GetByIdAsync(accountId);
        if (account == null) throw new Exception("Conta não encontrada.");

        // Simulação de geração de número de cartão (últimos 4 dígitos)
        var lastFour = new Random().Next(1000, 9999).ToString();

        // Holder name viria do User, mas por simplicidade usamos ID ou buscamos depois.
        // Em um cenário real, injetaríamos IUserRepository ou passaríamos o nome.
        var card = new AccountCard(accountId, tenantId, lastFour, brand, "CLIENT HOLDER", type, isVirtual, creditLimit);

        await _cardRepo.AddAsync(card);
        return card.Id;
    }
}
