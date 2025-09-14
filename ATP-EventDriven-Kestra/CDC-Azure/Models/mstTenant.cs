namespace CDC_Azure.Models
{
    public class mstTenant
    {
        public string TenantID { get; set; }
        public string SiteID { get; set; }
        public string CustomerID { get; set; }
        public string CustomerSiteID { get; set; }
        public string CustomerSiteName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
