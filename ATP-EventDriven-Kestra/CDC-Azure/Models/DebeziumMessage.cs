namespace CDC_Azure.Models
{
    public class DebeziumMessage<T>
    {
        public object schema { get; set; }
        public DebeziumPayload<T> payload { get; set; }
    }

}
