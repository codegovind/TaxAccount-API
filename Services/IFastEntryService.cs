using TaxAccount.Models.Accounting;

namespace TaxAccount.Services;

public interface IFastEntryService
{
    // Ledger Quick Create
    Task<AccountHead> QuickCreateLedgerAsync(string name, int groupId, int tenantId);
    
    // Contra Voucher (Cash <-> Bank)
    Task<VoucherEntry> CreateContraVoucherAsync(ContraVoucherDto dto, int tenantId, int userId);
    
    // Capital Entry
    Task<VoucherEntry> CreateCapitalEntryAsync(CapitalEntryDto dto, int tenantId, int userId);
    
    // Tax Set-off Calculation
    Task<TaxLiabilityDto> CalculateTaxLiabilityAsync(int tenantId, DateTime fromDate, DateTime toDate);
    Task<VoucherEntry> RecordTaxPaymentAsync(TaxPaymentDto dto, int tenantId, int userId);
    
    // Credit/Debit Note
    Task<VoucherEntry> CreateCreditNoteAsync(CreditNoteDto dto, int tenantId, int userId);
    Task<VoucherEntry> CreateDebitNoteAsync(DebitNoteDto dto, int tenantId, int userId);
}

public class ContraVoucherDto
{
    public DateTime Date { get; set; }
    public string Narration { get; set; } = string.Empty;
    public int FromAccountId { get; set; }  // Cash or Bank
    public int ToAccountId { get; set; }    // Bank or Cash
    public decimal Amount { get; set; }
}

public class CapitalEntryDto
{
    public DateTime Date { get; set; }
    public bool IsCapitalIntroduction { get; set; } // true = Capital In, false = Drawings
    public int AccountId { get; set; } // Cash/Bank/Asset account
    public decimal Amount { get; set; }
    public string Narration { get; set; } = string.Empty;
}

public class TaxLiabilityDto
{
    public decimal TotalOutputGst { get; set; } // From Sales
    public decimal TotalInputGst { get; set; }  // From Purchases
    public decimal NetLiability { get; set; }   // Output - Input
    public decimal CgstPayable { get; set; }
    public decimal SgstPayable { get; set; }
    public decimal IgstPayable { get; set; }
}

public class TaxPaymentDto
{
    public DateTime Date { get; set; }
    public decimal CgstPaid { get; set; }
    public decimal SgstPaid { get; set; }
    public decimal IgstPaid { get; set; }
    public int PaymentAccountId { get; set; } // Bank/Cash account used for payment
    public string ReferenceNumber { get; set; } = string.Empty;
}

public class CreditNoteDto
{
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int? OriginalInvoiceId { get; set; }
    public List<CreditNoteItemDto> Items { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
}

public class CreditNoteItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxPercent { get; set; }
}

public class DebitNoteDto
{
    public DateTime Date { get; set; }
    public int VendorId { get; set; }
    public int? OriginalBillId { get; set; }
    public List<DebitNoteItemDto> Items { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
}

public class DebitNoteItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxPercent { get; set; }
}
