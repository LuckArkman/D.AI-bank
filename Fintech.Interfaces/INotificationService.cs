namespace Fintech.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendSmsAsync(string phoneNumber, string message);
    Task NotifyTransactionAsync(Guid accountId, string message);
}
