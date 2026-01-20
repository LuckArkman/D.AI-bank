using Fintech.Interfaces;
using Fintech.Records;

namespace Fintech.Gateways;

public class FakePaymentGateway : IPaymentGateway
{
    public Task<PaymentResult> ProcessCreditCardAsync(CardData card, decimal amount)
    {
        throw new NotImplementedException();
    }

    public async Task<string> GenerateBoletoAsync(decimal amount, Guid accountId)
    {
        // Simula chamada externa
        await Task.Delay(200); 
        return $"34191.79001 01043.510047 {Guid.NewGuid()} 1 800000{amount}"; // Linha digitável fake
    }
}