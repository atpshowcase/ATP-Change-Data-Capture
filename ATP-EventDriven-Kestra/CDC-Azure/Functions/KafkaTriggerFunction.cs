using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CDC_Azure.Services;
using CDC_Azure.Config;
using CDC_Azure.Models;
using System.Text.Json;

namespace CDC_Azure.Functions
{
    public class KafkaTriggerFunction
    {
        private readonly ILogger<KafkaTriggerFunction> _logger;
        private readonly KafkaConsumerService _consumerService;
        private readonly DatabaseService _dbService;

        public KafkaTriggerFunction(ILogger<KafkaTriggerFunction> logger)
        {
            _logger = logger;
            _consumerService = new KafkaConsumerService(KafkaConfig.BootstrapServers, "your-topic-name");
            _dbService = new DatabaseService();
        }

        [Function(nameof(KafkaTriggerFunction))]
        public async Task Run([TimerTrigger("*/5 * * * * *")] TimerInfo timer)
        {
            _logger.LogInformation("Kafka Consumer Timer Triggered.");

            await _consumerService.StartConsuming(async (message) =>
            {
                // Deserialize message ke model MstOrder
                var order = JsonSerializer.Deserialize<mstOrder>(message);
                if (order != null)
                {
                    await _dbService.InsertOrUpdateMstOrder(order);
                    _logger.LogInformation($"Processed order {order.Id}");
                }
            });
        }
    }
}
