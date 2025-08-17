using Confluent.Kafka;
using CDC_Azure.Config;
using CDC_Azure.Helpers;
using CDC_Azure.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CDC_Azure.Consumers
{
    public class OrderConsumer
    {
        private readonly ILogger<OrderConsumer> _logger;
        private bool cdcEnabled = true;

        public OrderConsumer(ILogger<OrderConsumer> logger)
        {
            _logger = logger;
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
            consumer.Subscribe(KafkaConfig.Topic);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var result = consumer.Consume(token);
                    _logger.LogInformation($"📦 Received: {result.Message.Value}");

                    if (!cdcEnabled)
                    {
                        await Task.Delay(2000, token); // delay even if CDC is disabled
                        continue;
                    }

                    //var payload = JsonSerializer.Deserialize<DebeziumPayload<mstOrder>>(result.Message.Value);
                    var payload = JsonSerializer.Deserialize<DebeziumPayload<mstOrder>>(result.Message.Value, options);

                    var inputs = new Dictionary<string, object>
                    {
                        ["orderId"] = "salamander_445751",
                        ["status"] = payload.after?.SONumber ?? "",
                        ["before"] = payload.before,
                        ["after"] = payload.after,
                        ["source"] = payload.source,
                        ["op"] = payload.op,
                        ["ts_ms"] = payload.ts_ms
                    };


                    // Trigger the existing flow using its namespace, ID, and webhook key
                    //var execId = await KestraTrigger.TriggerExistingFlowWithWebhookAsync(
                    //    ns: "dev223", // The namespace of your deployed flow
                    //    flowId: "TBiGSys.dbo.mstOrder", // The ID of your deployed flow
                    //    webhookKey: "your_secret_and_unique_webhook_key_123", // The 'key' from your webhook trigger
                    //    inputs: inputs
                    //);

                    var execId = await KestraTrigger.TriggerFlowMultipartAsync(@"
                        id: salamander_445751
                        namespace: company.team
                        tasks:
                          - id: hello
                            type: io.kestra.plugin.core.log.Log
                            message: Hello World! 🚀
                        inputs:
                          - name: orderId
                            type: STRING
                          - name: status
                            type: STRING
                          - name: before
                            type: JSON
                          - name: after
                            type: JSON
                          - name: source
                            type: JSON
                          - name: op
                            type: STRING
                          - name: ts_ms
                            type: STRING", inputs
                    );

                    //var execId = await KestraTrigger.TriggerFlowMultipartAsync(
                    //    "company.team",
                    //    "salamander_445751",
                    //    inputs
                    //);

                    _ = Task.Run(() => KestraMonitor.MonitorExecutionAsync(execId, token));
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("🛑 Consumer stopped.");
            }

            await Task.Delay(2000, token); // ⏲️ Delay 2 seconds before next consume
        }

        public void EnableCDC(bool enabled) => cdcEnabled = enabled;
    }
}
