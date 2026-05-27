namespace TaxAccount.Models
{
    public enum InvoiceType
    {
        Sale = 1,
        Purchase = 2
    }

    public enum PaymentMethod
    {
        Cash = 1,
        UPI = 2,
        BankTransfer = 3,
        Credit = 4
    }

    public enum EntrySource
    {
        FullAccounting = 1,
        ComplianceOnly = 2
    }

    public class Invoice
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public InvoiceType InvoiceType { get; set; } = InvoiceType.Sale;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Credit;
        public EntrySource EntrySource { get; set; } = EntrySource.FullAccounting;
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }

        // Contact (Party) - nullable for cash sales
        public int? ContactId { get; set; }
        public int CreatedByUserId { get; set; }

        public string Notes { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Tenant Tenant { get; set; } = null!;
        public Contact? Contact { get; set; }
        public User CreatedBy { get; set; } = null!;
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public TransportDetail? TransportDetail { get; set; }
    }
}