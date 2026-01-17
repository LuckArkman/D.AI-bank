using Fintech.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class OutboxDispatcher : BackgroundService
{
    private readonly IServiceProvider _sp;

    public OutboxDispatcher(IServiceProvider sp) => _sp = sp;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _sp.CreateScope();
            var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

            var messages = await outboxRepo.GetPendingAsync(50);
            foreach (var msg in messages)
            {
                bus.Publish($"event.{msg.Type}", msg.Payload);
                await outboxRepo.MarkAsProcessedAsync(msg.Id);
            }
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}