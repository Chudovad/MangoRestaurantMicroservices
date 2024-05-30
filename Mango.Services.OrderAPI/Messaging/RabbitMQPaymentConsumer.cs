using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Repository;
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
        private readonly string paymentOrderUpdateQueueName;
        private readonly string paymentOrderRoutingKey;
         
        private readonly OrderRepository _orderRepository;
        string queueName = "";
        public RabbitMQPaymentConsumer(OrderRepository orderRepository, IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;

            exchangeName = _configuration.GetValue<string>("ExchangeName");
            paymentOrderUpdateQueueName = _configuration.GetValue<string>("PaymentOrderUpdateQueueName");
            paymentOrderRoutingKey = _configuration.GetValue<string>("PaymentOrderRoutingKey");

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            _channel.QueueDeclare(paymentOrderUpdateQueueName, false, false, false, null);
            _channel.QueueBind(paymentOrderUpdateQueueName, exchangeName, paymentOrderRoutingKey);
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
            _channel.BasicConsume(paymentOrderUpdateQueueName, false, consumer);

            return Task.CompletedTask;
        }

        private async Task HandleMessage(UpdatePaymentResultMessage updatePaymentResultMessage)
        {
            try
            {
                await _orderRepository.UpdateOrderPaymentStatus(updatePaymentResultMessage.OrderId,
                    updatePaymentResultMessage.Status);
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
