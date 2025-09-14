using CDC_Azure.Config;
using CDC_Azure.Helpers;
using CDC_Azure.Models;
using CDC_Azure.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CDC_Azure.Consumers
{
    public class TenantConsumer
    {
        private readonly ILogger<TenantConsumer> _logger;
        private readonly TenantService _tenantService;

        public TenantConsumer(ILogger<TenantConsumer> logger, TenantService orderService)
        {
            _logger = logger;
            _tenantService = orderService;
        }

        public async Task Start(CancellationToken token)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = KafkaConfig.BootstrapServers,
                GroupId = KafkaConfig.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var options = JsonConverterHelper.GetDefaultOptions();

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(KafkaConfig.TenantTopic);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var result = consumer.Consume(token);

                    var payload = JsonSerializer.Deserialize<DebeziumPayload<mstTenant>>(result.Message.Value, options);

                    var inputs = new Dictionary<string, object>
                    {
                        ["orderId"] = "hedgehog_996534",
                        ["status"] = payload.after?.SiteID ?? "",
                        ["before"] = payload.before,
                        ["after"] = payload.after,
                        ["source"] = payload.source,
                        ["op"] = payload.op,
                        ["ts_ms"] = payload.ts_ms
                    };

                    var execId = await KestraHelper.TriggerFlowTBiGSys(inputs);

                    await _tenantService.CreateTenant(execId, token);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Consumer stopped.");
            }

            await Task.Delay(2000, token);
        }
    }
}
