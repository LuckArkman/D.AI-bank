using Fintech.Interfaces;
using Microsoft.Extensions.Logging;

namespace Fintech.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Simulação de envio de e-mail
        _logger.LogInformation("Enviando e-mail para {To}: {Subject}", to, subject);
        await Task.Delay(100);
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        // Simulação de envio de SMS
        _logger.LogInformation("Enviando SMS para {PhoneNumber}: {Message}", phoneNumber, message);
        await Task.Delay(50);
    }

    public async Task NotifyTransactionAsync(Guid accountId, string message)
    {
        // Aqui poderíamos buscar o e-mail/telefone do usuário vinculado à conta e enviar a notificação
        _logger.LogInformation("Notificação de transação para conta {AccountId}: {Message}", accountId, message);
        await Task.Delay(10);
    }
}
