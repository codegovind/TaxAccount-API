namespace TaxAccount.Models.Accounting
{
    public enum VoucherType
    {
        Payment = 1,
        Receipt = 2
    }

    public class Payment
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string VoucherNumber { get; set; } = string.Empty;
        public VoucherType VoucherType { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int? ContactId { get; set; }
        public int CreatedByUserId { get; set; }
        public string Narration { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public bool IsReconciled { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; } = null!;
        public Contact? Contact { get; set; }
        public User CreatedBy { get; set; } = null!;
    }
}
