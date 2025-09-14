using CDC_Azure.Config;
using CDC_Azure.Helpers;
using CDC_Azure.Models;
using CDC_Azure.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CDC_Azure.Consumers
{
    public class EmailConsumer
    {
        private readonly ILogger<EmailConsumer> _logger;
        private readonly OrderService _orderService;

        public EmailConsumer(ILogger<EmailConsumer> logger, OrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        public async Task Start(CancellationToken token)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = KafkaConfig.BootstrapServers,
                GroupId = KafkaConfig.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            var options = JsonConverterHelper.GetDefaultOptions();

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(KafkaConfig.EmailTopic);

            _logger.LogInformation($"[Kafka] Subscribed to topic: {KafkaConfig.EmailTopic}");

            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(token);

                        //// Log pesan mentah dari Kafka
                        _logger.LogInformation($"[Kafka] Raw message: {result.Message.Value}");

                        //using var jsonDoc = JsonDocument.Parse(result.Message.Value);

                        //// Kalau pesan heartbeat, tidak ada payload → skip
                        //if (!jsonDoc.RootElement.TryGetProperty("payload", out var payloadElement))
                        //{
                        //    _logger.LogWarning("[Kafka] Message does not have payload. Skipping.");
                        //    continue;
                        //}

                        //// Deserialize payload ke dalam model
                        //DebeziumPayload<mstOrder>? payload;
                        //try
                        //{
                        //    payload = JsonSerializer.Deserialize<DebeziumPayload<mstOrder>>(payloadElement.ToString(), options);
                        //}
                        //catch (JsonException ex)
                        //{
                        //    _logger.LogError($"[Kafka] Failed to deserialize payload: {ex.Message}");
                        //    _logger.LogError($"[Kafka] Payload raw: {payloadElement}");
                        //    continue;
                        //}

                        //// Kalau after == null → berarti delete event atau snapshot
                        //if (payload?.after == null)
                        //{
                        //    _logger.LogInformation("[Kafka] No 'after' data found, skipping message.");
                        //    continue;
                        //}

                        //// Build input untuk Kestra
                        //var inputs = new Dictionary<string, object>
                        //{
                        //    ["orderId"] = payload.after.SONumber ?? "",
                        //    //["status"] = payload.after.Status ?? "",
                        //    ["status"] = payload.after?.SONumber ?? "",
                        //    ["before"] = payload.before,
                        //    ["after"] = payload.after,
                        //    ["source"] = payload.source,
                        //    ["op"] = payload.op,
                        //    ["ts_ms"] = payload.ts_ms
                        //};

                        //// Trigger flow Kestra
                        //var execId = await KestraHelper.TriggerFlowTBiGSys(inputs);
                        //_logger.LogInformation($"[Kestra] Flow triggered. ExecID: {execId}");

                        //// Simpan order
                        //await _orderService.CreateOrder(execId, token);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError($"[Kafka] Consume error: {ex.Error.Reason}");
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError($"[Kafka] JSON parse error: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[Kafka] Unexpected error: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("EmailConsumer stopped.");
            }
            finally
            {
                consumer.Close();
                _logger.LogInformation("Kafka consumer closed.");
            }
        }
    }
}
