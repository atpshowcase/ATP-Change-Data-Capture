namespace CDC_Azure.Models
{
    public class SourceMetadata
    {
        public string version { get; set; }
        public string connector { get; set; }
        public string name { get; set; }
        public long ts_ms { get; set; }
        public string db { get; set; }
        public string schema { get; set; }
        public string table { get; set; }
        public string change_lsn { get; set; }
        public string commit_lsn { get; set; }
        public int? event_serial_no { get; set; }
    }
}
