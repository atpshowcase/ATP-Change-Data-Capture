namespace CDC_Azure.Models
{
    public class mstOrder
    {
        public int ID { get; set; }
        public string SONumber { get; set; }
        public string Product { get; set; }
        public int? RegionID { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
