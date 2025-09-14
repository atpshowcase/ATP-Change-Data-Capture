namespace CDC_Azure.Models
{
    public class mstOrder
    {
        public string SONumber { get; set; }
        public string TenantID { get; set; }
        public int? ProductID { get; set; }
        public int? SOWCustomerProductID { get; set; }
        public int? ColoTypeID { get; set; }
        public string CompanyID { get; set; }
        public string AccountManagerID { get; set; }
        public string LeadProjectManagerID { get; set; }
        public string ProjectDirectorPDIID { get; set; }
        public string SitacOfficerID { get; set; }
        public string FieldControllerID { get; set; }
        public string SitacSpecialistID { get; set; }
        public decimal? PLNPowerKVA { get; set; }
        public int? SitacTypeID { get; set; }
        public int? ShelterTypeID { get; set; }
        public string PMSitac { get; set; }
        public string PMCME { get; set; }
        public int? STIPApprovalStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
    }
}
