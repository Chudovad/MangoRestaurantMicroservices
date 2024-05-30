using Mango.Services.Email.Messages;
using Mango.Services.Email.Repository;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.OrderAPI.Messaging
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IConfiguration _configuration;
        private readonly string exchangeName;
        private readonly string paymentEmailUpdateQueueName;
        private readonly string paymentEmailRoutingKey;
        private readonly EmailRepository _emailRepo;
        string queueName = "";

        public RabbitMQPaymentConsumer(EmailRepository emailRepo, IConfiguration configuration)
        {

            _emailRepo = emailRepo;
            _configuration = configuration;

            exchangeName = _configuration.GetValue<string>("ExchangeName");
            paymentEmailUpdateQueueName = _configuration.GetValue<string>("PaymentEmailUpdateQueueName");
            paymentEmailRoutingKey = _configuration.GetValue<string>("PaymentEmailRoutingKey");

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            _channel.QueueDeclare(paymentEmailUpdateQueueName, false, false, false, null);
            _channel.QueueBind(paymentEmailUpdateQueueName, exchangeName, paymentEmailRoutingKey);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                UpdatePaymentResultMessage updatePaymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(content);
                HandleMessage(updatePaymentResultMessage).GetAwaiter().GetResult();

                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(paymentEmailUpdateQueueName, false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandleMessage(UpdatePaymentResultMessage updatePaymentResultMessage)
        {
            try
            {
                await _emailRepo.SendAndLogEmail(updatePaymentResultMessage);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
