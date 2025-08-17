namespace CDC_Azure.Config
{
    public static class SqlConfig
    {
        public static string ConnectionString => Environment.GetEnvironmentVariable("SqlConnectionString");
    }
}
