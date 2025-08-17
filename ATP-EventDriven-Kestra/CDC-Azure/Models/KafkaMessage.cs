namespace CDC_Azure.Models
{
    public class KafkaMessage
    {
        public int Offset { get; set; }
        public int Partition { get; set; }
        public string Topic { get; set; }
        public DateTime Timestamp { get; set; }
        public string Value { get; set; } // ini STRING, bukan object
        public string Key { get; set; }
        public List<object> Headers { get; set; }
    }
}
