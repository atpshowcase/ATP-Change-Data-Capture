using Confluent.Kafka;
using CDC_Azure.Services;
using System.Text.Json;
using CDC_Azure.Models;
using System.Text.Json.Serialization;
using CDC_Azure.Helpers;
using System.Diagnostics;

// Ganti options manual jadi:


namespace CDC_Azure.Consumers
{
    public abstract class BaseConsumer<T> where T : class
    {
        private readonly DatabaseService _dbService;
        private readonly string _topic;

        protected BaseConsumer(string topic)
        {
            _dbService = new DatabaseService();
            _topic = topic;
        }

        public void Start(CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = Config.KafkaConfig.BootstrapServers,
                GroupId = Config.KafkaConfig.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            var options = JsonConverterHelper.GetDefaultOptions();

            consumer.Subscribe(_topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    if (consumeResult?.Message != null)
                    {
                        Debug.WriteLine(message: $"Subscribe Ready to Use...");
                        Debug.WriteLine($"Message received from {_topic}: {consumeResult.Message.Value}");

                        var kafkaMessage = JsonSerializer.Deserialize<DebeziumPayload<T>>(consumeResult.Message.Value, options);
                        if (kafkaMessage?.source != null)
                        {
                            var json = JsonSerializer.Serialize(kafkaMessage.after);
                            var model = JsonSerializer.Deserialize<T>(json, options);

                            if (model != null)
                                HandleMessage(model);
                        }
                    }
                }
                catch (ConsumeException e)
                {
                    Debug.WriteLine($"Error occurred while consuming message: {e.Message}");
                }
            }

            consumer.Close();
        }


        // Declare HandleMessage as an abstract method, to be implemented by subclasses.
        protected abstract Task HandleMessage(T data);
    }
}
