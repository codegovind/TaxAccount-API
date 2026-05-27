namespace TaxAccount.Models.Accounting;

public enum VoucherType
{
    Sales = 1,
    Purchase = 2,
    Payment = 3,
    Receipt = 4,
    Contra = 5,      // Cash <-> Bank
    Journal = 6,     // Non-cash adjustments
    CreditNote = 7,  // Sales Return
    DebitNote = 8,   // Purchase Return
    Capital = 9      // Capital Introduction/Drawings
}

public class VoucherEntry
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public VoucherType VoucherType { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    
    public int AccountHeadId { get; set; }
    public AccountHead AccountHead { get; set; } = null!;
    
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    
    public string Narration { get; set; } = string.Empty;
    
    // For Contra: Link to other cash/bank account if needed
    public int? RelatedAccountId { get; set; }
    
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
