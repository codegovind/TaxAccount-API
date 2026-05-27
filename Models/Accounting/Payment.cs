namespace TaxAccount.Models.Accounting;

public enum VoucherType
{
    Sales = 1,
    Purchase = 2,
    Payment = 3,
    Receipt = 4,
    Journal = 5,
    CreditNote = 6,
    DebitNote = 7
}

public class Payment
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public VoucherType VoucherType { get; set; } // Payment or Receipt
    public string VoucherNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    // Party (Contact)
    public int? ContactId { get; set; }
    public Contact? Contact { get; set; }
    
    // Payment Details
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? ReferenceNumber { get; set; } // Cheque No, UTR, etc.
    public DateTime? ReferenceDate { get; set; }
    public string Narration { get; set; } = string.Empty;
    
    // Link to Invoice/Bill if applicable
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    
    public int? PurchaseBillId { get; set; }
    public PurchaseBill? PurchaseBill { get; set; }
    
    public int CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation to Ledger Entries
    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
}
