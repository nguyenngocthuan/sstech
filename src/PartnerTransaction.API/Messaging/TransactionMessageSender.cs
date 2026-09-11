using PartnerTransaction.API.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PartnerTransaction.API.Messaging
{
    public interface TransactionMessageSender
    {
        Task SendTransactionMessageAsync(PartnerTransactionRequestModel transactionRequest, CancellationToken cancellationToken = default);
    }

    public class TransactionMessageSenderImp(IConfiguration configuration) : TransactionMessageSender
    {
        public async Task SendTransactionMessageAsync(PartnerTransactionRequestModel transactionRequest, CancellationToken cancellationToken = default)
        {
            var queueName = configuration["RabbitMQ:HostName"] ?? "partner-transactions";
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
                Port = int.Parse(configuration["RabbitMQ:Port"]!)
            };

            await using var connection =
                await factory.CreateConnectionAsync(cancellationToken);

            await using var channel =
                await connection.CreateChannelAsync(
                    cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var json = JsonSerializer.Serialize(transactionRequest);

            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
