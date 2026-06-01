namespace TaxAccount.Models.Compliance
{
    public class TenantSetting
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public bool IsEWayBillEnabled { get; set; } = false;
        public bool IsAccountingEnabled { get; set; } = true;
        public bool IsComplianceEnabled { get; set; } = false;
        public string DefaultPaymentMethod { get; set; } = "Credit";
        public int DefaultCreditDays { get; set; } = 30;
        public bool AutoGenerateInvoiceNumber { get; set; } = true;
        public string InvoiceNumberPrefix { get; set; } = "INV-";
        public bool AutoPostToAccounting { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; } = null!;
    }
}
