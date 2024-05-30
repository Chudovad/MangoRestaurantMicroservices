using Mango.Services.PaymentAPI.Messaging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.PaymentAPI.RabbitMQSender
{
    public class RabbitMQPaymentMessageSender : IRabbitMQPaymentMessageSender
    {
        private readonly string _hostname;
        private readonly string _password;
        private readonly string _username;
        private IConnection _connection;
        private readonly IConfiguration _configuration;
        private readonly string exchangeName;
        private readonly string paymentEmailUpdateQueueName;
        private readonly string paymentOrderUpdateQueueName;
        private readonly string paymentOrderRoutingKey;
        private readonly string paymentEmailRoutingKey;

        public RabbitMQPaymentMessageSender(IConfiguration configuration)
        {
            _configuration = configuration;
            exchangeName = _configuration.GetValue<string>("ExchangeName");
            paymentEmailUpdateQueueName = _configuration.GetValue<string>("PaymentEmailUpdateQueueName");
            paymentOrderUpdateQueueName = _configuration.GetValue<string>("PaymentOrderUpdateQueueName");
            paymentOrderRoutingKey = _configuration.GetValue<string>("PaymentEmailRoutingKey");
            paymentEmailRoutingKey = _configuration.GetValue<string>("PaymentOrderRoutingKey");
            _hostname = "localhost";
            _password = "guest";
            _username = "guest";
        }

        public void SendMessage(BaseMessage message)
        {
            if (ConnectionExists())
            {
                using var channel = _connection.CreateModel();
                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: false);
                channel.QueueDeclare(paymentOrderUpdateQueueName, false, false, false, null);
                channel.QueueDeclare(paymentEmailUpdateQueueName, false, false, false, null);

                channel.QueueBind(paymentEmailUpdateQueueName, exchangeName, paymentEmailRoutingKey);
                channel.QueueBind(paymentOrderUpdateQueueName, exchangeName, paymentOrderRoutingKey);

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange: exchangeName, paymentEmailRoutingKey, basicProperties: null, body: body);
                channel.BasicPublish(exchange: exchangeName, paymentOrderRoutingKey, basicProperties: null, body: body);
            }
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };
                _connection = factory.CreateConnection();
            }
            catch (Exception)
            {
                //log exception
            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }
            CreateConnection();
            return _connection != null;
        }
    }
}
