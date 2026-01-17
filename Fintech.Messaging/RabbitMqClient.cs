using RabbitMQ.Client;
using System.Text;
using Fintech.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Fintech.Messaging;

public class RabbitMqClient : IMessageBus, IDisposable
{
    private IChannel? _channel;
    private IConnection? _connection;
    private readonly ConnectionFactory _factory;

    public RabbitMqClient(IConfiguration config)
    {
        _factory = new ConnectionFactory() { HostName = config["RabbitMQ:Host"] ?? "localhost" };
    }

    private async Task EnsureConnected()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.ExchangeDeclareAsync("fintech.events", ExchangeType.Topic);
        }
    }

    public async Task PublishAsync(string routingKey, string message)
    {
        await EnsureConnected();
        var body = Encoding.UTF8.GetBytes(message);
        await _channel!.BasicPublishAsync("fintech.events", routingKey, body);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}