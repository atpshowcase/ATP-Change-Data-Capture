namespace CDC_Azure.Models
{
    public class mstTenant
    {
        public int id { get; set; }
        public string tenant_name { get; set; }
        public string address { get; set; }
        public string phone_number { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
