using Mango.Services.PaymentAPI.Messages;
using Mango.Services.PaymentAPI.RabbitMQSender;
using Newtonsoft.Json;
using PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IRabbitMQPaymentMessageSender _rabbitMQPaymentMessageSender;
        private readonly IProcessPayment _processPayment;
        private readonly IConfiguration _configuration;
        public readonly string orderPaymentQueueName;

        public RabbitMQPaymentConsumer(IRabbitMQPaymentMessageSender rabbitMQPaymentMessageSender,
            IProcessPayment processPayment, IConfiguration configuration)
        {
            _processPayment = processPayment;
            _rabbitMQPaymentMessageSender = rabbitMQPaymentMessageSender;
            _configuration = configuration;

            orderPaymentQueueName = _configuration.GetValue<string>("OrderPaymentQueueName");

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: orderPaymentQueueName, false, false, false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                PaymentRequestMessage paymentRequestMessage = JsonConvert.DeserializeObject<PaymentRequestMessage>(content);
                HandleMessage(paymentRequestMessage).GetAwaiter().GetResult();

                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(orderPaymentQueueName, false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandleMessage(PaymentRequestMessage paymentRequestMessage)
        {
            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage updatePaymentResultMessage = new()
            {
                Status = result,
                OrderId = paymentRequestMessage.OrderId,
                Email = paymentRequestMessage.Email
            };


            try
            {
                _rabbitMQPaymentMessageSender.SendMessage(updatePaymentResultMessage);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
