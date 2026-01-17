using Fintech.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fintech.Worker;

public class OutboxWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxWorker> _logger;

    public OutboxWorker(IServiceProvider serviceProvider, ILogger<OutboxWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

                var messages = await outboxRepo.GetPendingAsync(20);

                foreach (var msg in messages)
                {
                    try
                    {
                        await bus.PublishAsync(msg.Topic, msg.PayloadJson);
                        await outboxRepo.MarkAsProcessedAsync(msg.Id);
                        _logger.LogDebug("Mensagem outbox {Id} processada.", msg.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar mensagem outbox {Id}.", msg.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no ciclo do OutboxWorker.");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}