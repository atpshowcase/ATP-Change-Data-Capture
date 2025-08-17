namespace CDC_Azure.Config
{
    public static class KafkaConfig
    {
        public const string BootstrapServers = "localhost:9092";
        public const string Topic = "fullfillment.TBiGSys.dbo.mstOrder";
        public const string GroupId = "cdc-order-consumer";
    }
}
