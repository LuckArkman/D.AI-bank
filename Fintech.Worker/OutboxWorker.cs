using Fintech.Entities;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Fintech.Worker;

public class OutboxWorker : BackgroundService
{
    private readonly IMongoCollection<OutboxMessage> _outbox;
    // Injetar IBus ou KafkaProducer aqui

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Pega mensagens não processadas
            var messages = await _outbox.Find(x => !x.Processed)
                .Limit(50)
                .ToListAsync();

            foreach (var msg in messages)
            {
                try 
                {
                    // 1. Publicar no RabbitMQ/Kafka
                    // await _bus.Publish(msg.Topic, msg.PayloadJson);

                    // 2. Marcar como processado (ou deletar)
                    await _outbox.UpdateOneAsync(
                        x => x.Id == msg.Id,
                        Builders<OutboxMessage>.Update.Set(x => x.Processed, true)
                    );
                }
                catch
                {
                    // Logar erro, mas não travar o loop. O retry vai pegar na próxima.
                    // Idealmente implementar Backoff Exponencial aqui.
                }
            }

            await Task.Delay(1000, stoppingToken); // Polling
        }
    }
}