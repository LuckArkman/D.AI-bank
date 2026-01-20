using Fintech.Records;

namespace Fintech.Interfaces;
public interface IPaymentGateway
{
    Task<PaymentResult> ProcessCreditCardAsync(CardData card, decimal amount);
    Task<string> GenerateBoletoAsync(decimal amount, Guid accountId);
}