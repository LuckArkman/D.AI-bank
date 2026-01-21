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
        var workerId = Guid.NewGuid().ToString();
        _logger.LogInformation("OutboxWorker iniciado com ID: {WorkerId}", workerId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

                // Uso do lock para suportar múltiplos workers com segurança
                var messages = await outboxRepo.LockAndGetAsync(20, workerId);

                foreach (var msg in messages)
                {
                    try
                    {
                        await bus.PublishAsync(msg.Topic, msg.PayloadJson);
                        await outboxRepo.MarkAsProcessedAsync(msg.Id);
                        _logger.LogDebug("Mensagem outbox {Id} processada pelo worker {WorkerId}.", msg.Id, workerId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar mensagem outbox {Id}. Liberando lock.", msg.Id);
                        await outboxRepo.UnlockAsync(msg.Id);
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