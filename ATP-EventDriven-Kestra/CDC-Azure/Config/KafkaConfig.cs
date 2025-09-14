namespace CDC_Azure.Config
{
    public static class KafkaConfig
    {
        public const string BootstrapServers = "localhost:9092";
        public const string GroupId = "cdc-order-consumer";

        public const string OrderTopic = "fullfillment_mstOrder.TBiGSys.dbo.mstOrder";
        public const string TenantTopic = "fullfillment_mstTenant.TBiGSys.dbo.mstTenant";


        public const string EmailTopic = "sendEmail";
    }
}
