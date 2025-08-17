using Confluent.Kafka;

namespace CDC_Azure.Services
{
    public class KafkaConsumerService
    {
        private readonly string _bootstrapServers;
        private readonly string _topicName;

        public KafkaConsumerService(string bootstrapServers, string topicName)
        {
            _bootstrapServers = bootstrapServers;
            _topicName = topicName;
        }

        public async Task StartConsuming(Func<string, Task> handleMessage)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "cdc-azure-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_topicName);

            while (true)
            {
                var consumeResult = consumer.Consume();
                await handleMessage(consumeResult.Message.Value);
            }
        }
    }
}
