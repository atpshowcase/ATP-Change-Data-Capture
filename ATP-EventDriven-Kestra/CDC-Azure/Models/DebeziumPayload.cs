namespace CDC_Azure.Models
{
    public class DebeziumPayload<T>
    {
        public T before { get; set; }
        public T after { get; set; }
        public string op { get; set; }
        public long ts_ms { get; set; }
        public SourceMetadata source { get; set; }
    }

}
