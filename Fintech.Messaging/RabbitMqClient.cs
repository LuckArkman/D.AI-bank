using RabbitMQ.Client;
using System.Text;
using Fintech.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace Fintech.Messaging;

public class RabbitMqClient : IMessageBus, IDisposable
{
    private readonly IModel _channel;
    private readonly IConnection _connection;

    public RabbitMqClient(IConfiguration config)
    {
        var factory = new ConnectionFactory() { HostName = config["RabbitMQ:Host"] };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare("fintech.events", ExchangeType.Topic);
    }

    public void Publish(string routingKey, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish("fintech.events", routingKey, null, body);
    }

    public void Dispose() { _channel?.Close(); _connection?.Close(); }
}