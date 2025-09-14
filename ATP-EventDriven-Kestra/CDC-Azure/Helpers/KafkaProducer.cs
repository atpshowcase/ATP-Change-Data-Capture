using Confluent.Kafka;
using System;
using System.Threading.Tasks;

public class KafkaProducer
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(string bootstrapServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task SendEmailMessageAsync(string message)
    {
        string topic = "sendEmail";
        try
        {
            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(), // optional key
                Value = message
            });

            Console.WriteLine($"[INFO] Pesan terkirim ke topic {topic}, partition {result.Partition}, offset {result.Offset}");
        }
        catch (ProduceException<string, string> ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] Gagal mengirim pesan: {ex.Error.Reason}");
            Console.ResetColor();
        }
    }
}
