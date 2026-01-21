using Fintech.Enums;
using Fintech.Interfaces;

namespace Fintech.Commands;

public class CardActionHandler
{
    private readonly ICardRepository _cardRepo;

    public CardActionHandler(ICardRepository cardRepo)
    {
        _cardRepo = cardRepo;
    }

    public async Task HandleToggleBlock(Guid cardId, Guid accountId)
    {
        var card = await _cardRepo.GetByIdAsync(cardId);

        if (card == null || card.AccountId != accountId)
            throw new Exception("Cartão não encontrado.");

        if (card.Status == CardStatus.Active)
        {
            card.Block();
        }
        else if (card.Status == CardStatus.Blocked)
        {
            card.Unblock();
        }

        await _cardRepo.UpdateAsync(card);
    }

    public async Task HandleUpdateLimit(Guid cardId, Guid accountId, decimal newLimit)
    {
        var card = await _cardRepo.GetByIdAsync(cardId);

        if (card == null || card.AccountId != accountId)
            throw new Exception("Cartão não encontrado.");

        if (card.Type != CardType.Credit && card.Type != CardType.Hybrid)
            throw new Exception("Somente cartões de crédito permitem ajuste de limite.");

        card.UpdateLimit(newLimit);
        await _cardRepo.UpdateAsync(card);
    }
}

