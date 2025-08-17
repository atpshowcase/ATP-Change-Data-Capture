using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace CDC_Azure
{
    public class KafkaTriggerFunction
    {
        private readonly ILogger<KafkaTriggerFunction> _logger;

        public KafkaTriggerFunction(ILogger<KafkaTriggerFunction> logger)
        {
            _logger = logger;
        }

        [Function("KafkaTriggerFunction")]
        public async Task RunAsync([KafkaTrigger(
    brokerList: "host.docker.internal:9092",
    topic: "fullfillment.Test_DB.dbo.prospects",
    ConsumerGroup = "$Default",
    Protocol = BrokerProtocol.Plaintext)] string kafkaEvent)
        {
            _logger.LogInformation($"📨 Kafka trigger received message: {kafkaEvent}");

            var sqlConnectionString = "Server=host.docker.internal,1433;Database=Test_DB;User Id=anan;Password=anan;TrustServerCertificate=True;";

            try
            {
                using (SqlConnection conn = new SqlConnection(sqlConnectionString))
                {
                    await conn.OpenAsync();

                    var outerDocument = JsonDocument.Parse(kafkaEvent);
                    var outerRoot = outerDocument.RootElement;

                    string offset = outerRoot.GetProperty("Offset").ToString();
                    string partition = outerRoot.GetProperty("Partition").ToString();
                    string topic = outerRoot.GetProperty("Topic").ToString();
                    string timestamp = outerRoot.GetProperty("Timestamp").ToString();
                    string valueStr = outerRoot.GetProperty("Value").ToString();
                    string keyStr = outerRoot.GetProperty("Key").ToString();


                    // 1. Insert ke KafkaLogs
                    var insertLogCmd = new SqlCommand("INSERT INTO KafkaLogs (Offset, Partition, Topic, Timestamp, Value, Key) VALUES (@offset, @partition, @topic, @timestamp, @value, @key)", conn);
                    insertLogCmd.Parameters.AddWithValue("@offset", offset);
                    insertLogCmd.Parameters.AddWithValue("@partition", partition);
                    insertLogCmd.Parameters.AddWithValue("@topic", topic);
                    insertLogCmd.Parameters.AddWithValue("@timestamp", timestamp);
                    insertLogCmd.Parameters.AddWithValue("@value", valueStr);
                    insertLogCmd.Parameters.AddWithValue("@key", keyStr);
                    await insertLogCmd.ExecuteNonQueryAsync();

                    _logger.LogInformation("Inserted into KafkaLogs ✅");

                    if (!outerRoot.TryGetProperty("Value", out var valueElement))
                    {
                        _logger.LogWarning("⚠️ No 'Value' field in Kafka message.");
                        return;
                    }

                    var innerJson = valueElement.GetString(); // String JSON dari Value
                    if (string.IsNullOrEmpty(innerJson))
                    {
                        _logger.LogWarning("⚠️ 'Value' field is empty.");
                        return;
                    }

                    var innerDocument = JsonDocument.Parse(innerJson);
                    var root = innerDocument.RootElement;

                    var operation = root.GetProperty("op").GetString(); // "c", "u", "d"

                    if (operation == "c" || operation == "u")
                    {
                        if (root.TryGetProperty("after", out var after) && after.ValueKind == JsonValueKind.Object)
                        {
                            var id = after.GetProperty("id").GetInt32();
                            var firstName = after.GetProperty("first_name").GetString();
                            var lastName = after.GetProperty("last_name").GetString();
                            var email = after.GetProperty("email").GetString();

                            if (operation == "c")
                            {
                                var insertQuery = @"INSERT INTO tempProspect (id, first_name, last_name, email)
                                            VALUES (@id, @firstName, @lastName, @email)";
                                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                                {
                                    cmd.Parameters.AddWithValue("@id", id);
                                    cmd.Parameters.AddWithValue("@firstName", firstName);
                                    cmd.Parameters.AddWithValue("@lastName", lastName);
                                    cmd.Parameters.AddWithValue("@email", email);
                                    await cmd.ExecuteNonQueryAsync();
                                }
                                _logger.LogInformation("✅ Inserted new record into tempProspect.");
                            }
                            else if (operation == "u")
                            {
                                var updateQuery = @"UPDATE tempProspect
                                            SET first_name = @firstName, last_name = @lastName, email = @email
                                            WHERE id = @id";
                                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                                {
                                    cmd.Parameters.AddWithValue("@id", id);
                                    cmd.Parameters.AddWithValue("@firstName", firstName);
                                    cmd.Parameters.AddWithValue("@lastName", lastName);
                                    cmd.Parameters.AddWithValue("@email", email);
                                    var rowsAffected = await cmd.ExecuteNonQueryAsync();

                                    if (rowsAffected == 0)
                                    {
                                        _logger.LogWarning("⚠️ No rows updated. Trying to insert instead.");
                                        var insertQuery = @"INSERT INTO tempProspect (id, first_name, last_name, email)
                                                    VALUES (@id, @firstName, @lastName, @email)";
                                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                                        {
                                            insertCmd.Parameters.AddWithValue("@id", id);
                                            insertCmd.Parameters.AddWithValue("@firstName", firstName);
                                            insertCmd.Parameters.AddWithValue("@lastName", lastName);
                                            insertCmd.Parameters.AddWithValue("@email", email);
                                            await insertCmd.ExecuteNonQueryAsync();
                                        }
                                    }
                                }
                                _logger.LogInformation("✅ Updated record in tempProspect.");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ No 'after' field found in the message.");
                        }
                    }
                    else if (operation == "d")
                    {
                        if (root.TryGetProperty("before", out var before) && before.ValueKind == JsonValueKind.Object)
                        {
                            var id = before.GetProperty("id").GetInt32();

                            var deleteQuery = @"DELETE FROM tempProspect WHERE id = @id";
                            using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                await cmd.ExecuteNonQueryAsync();
                            }
                            _logger.LogInformation($"✅ Deleted record with ID {id} from tempProspect.");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"ℹ️ Unsupported operation type: {operation}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error processing Kafka event: {ex.Message}");
                throw;
            }
        }

    }
}
