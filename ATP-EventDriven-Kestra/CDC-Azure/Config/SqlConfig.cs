namespace CDC_Azure.Config
{
    public static class SqlConfig
    {
        public const string Server = "DESKTOP-MLB67P9\\ATP";          
        public const string Database = "TBiGSys";         
        public const string User = "anan";                  
        public const string Password = "anan"; 
        public const bool TrustedConnection = false;      

        public static readonly string SqlConnectionString =
            $"Server={Server};Database={Database};User Id={User};Password={Password};Trusted_Connection={TrustedConnection};Encrypt=False;";
    }
}
