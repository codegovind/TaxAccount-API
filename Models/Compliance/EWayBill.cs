namespace TaxAccount.Models.Compliance
{
    public class EWayBill
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int InvoiceId { get; set; }
        public string EWayBillNumber { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
        public DateTime ValidUntil { get; set; }
        public string Irn { get; set; } = string.Empty;
        public string JsonData { get; set; } = string.Empty;
        public bool IsCancelled { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; } = null!;
        public Invoice Invoice { get; set; } = null!;
    }
}
