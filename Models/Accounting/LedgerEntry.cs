namespace TaxAccount.Models.Accounting
{
    public class LedgerEntry
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int AccountHeadId { get; set; }
        public DateTime Date { get; set; }
        public string VoucherType { get; set; } = string.Empty;
        public int VoucherId { get; set; }
        public string VoucherNumber { get; set; } = string.Empty;
        public string Narration { get; set; } = string.Empty;
        public decimal Debit { get; set; } = 0;
        public decimal Credit { get; set; } = 0;
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Tenant Tenant { get; set; } = null!;
        public AccountHead AccountHead { get; set; } = null!;
        public User CreatedBy { get; set; } = null!;
    }
}
