namespace TaxAccount.Models
{
    public class PurchaseBill
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Credit;
        public EntrySource EntrySource { get; set; } = EntrySource.FullAccounting;
        public DateTime BillDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public string VendorBillNumber { get; set; } = string.Empty;

        // Vendor (Party) - nullable for cash purchases
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
        public ICollection<PurchaseBillItem> Items { get; set; } = new List<PurchaseBillItem>();
    }

    public class PurchaseBillItem
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int PurchaseBillId { get; set; }
        public int ProductId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string HsnCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "Nos";
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal CgstPercent { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstPercent { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstPercent { get; set; }
        public decimal IgstAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Navigation
        public PurchaseBill PurchaseBill { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
