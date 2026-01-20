using Fintech.Entities;
using Fintech.Enums;
using Fintech.Interfaces;

namespace Fintech.Commands;

public class IssueCardHandler
{
    private readonly ICardRepository _cardRepo;
    private readonly IAccountRepository _accountRepo;

    public IssueCardHandler(ICardRepository cardRepo, IAccountRepository accountRepo)
    {
        _cardRepo = cardRepo;
        _accountRepo = accountRepo;
    }

    public async Task<Guid> Handle(Guid accountId, string brand, CardType type, bool isVirtual, decimal creditLimit = 0)
    {
        var account = await _accountRepo.GetByIdAsync(accountId);
        if (account == null) throw new Exception("Conta não encontrada.");

        // Simulação de geração de número de cartão (últimos 4 dígitos)
        var lastFour = new Random().Next(1000, 9999).ToString();

        // Holder name viria do User, mas por simplicidade usamos ID ou buscamos depois.
        // Em um cenário real, injetaríamos IUserRepository ou passaríamos o nome.
        var card = new AccountCard(accountId, lastFour, brand, "CLIENT HOLDER", type, isVirtual, creditLimit);

        await _cardRepo.AddAsync(card);
        return card.Id;
    }
}
