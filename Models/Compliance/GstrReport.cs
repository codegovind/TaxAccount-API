namespace TaxAccount.Models.Compliance;

/// <summary>
/// GSTR-1 Report Model - Outward Supplies (Sales) for GST Portal
/// Auto-generated from Invoices for monthly/quarterly filing
/// </summary>
public class Gstr1Report
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    // Reporting period
    public string TaxPeriod { get; set; } = string.Empty;  // Format: "MMYYYY" e.g., "012024"
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    // Filing status
    public GstrFilingStatus Status { get; set; } = GstrFilingStatus.Draft;
    public DateTime? GeneratedDate { get; set; }
    public DateTime? FiledDate { get; set; }
    public string? ArnNumber { get; set; }  // Application Reference Number after filing
    
    // Summary totals (auto-calculated)
    public decimal TotalTaxableValue { get; set; }
    public decimal TotalCgst { get; set; }
    public decimal TotalSgst { get; set; }
    public decimal TotalIgst { get; set; }
    public decimal TotalCess { get; set; }
    public decimal GrandTotal { get; set; }
    
    // Section-wise data
    public ICollection<Gstr1B2bInvoice> B2bInvoices { get; set; } = new List<Gstr1B2bInvoice>();  // 4A - B2B
    public ICollection<Gstr1B2cLargeInvoice> B2cLargeInvoices { get; set; } = new List<Gstr1B2cLargeInvoice>();  // 5A - B2C Large
    public ICollection<Gstr1B2cSmallInvoice> B2cSmallInvoices { get; set; } = new List<Gstr1B2cSmallInvoice>();  // 7 - B2C Small
    public ICollection<Gstr1CreditDebitNote> CreditDebitNotes { get; set; } = new List<Gstr1CreditDebitNote>();  // 9A - CDNR
    public ICollection<Gstr1ExportInvoice> ExportInvoices { get; set; } = new List<Gstr1ExportInvoice>();  // 6A - Exports
    
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum GstrFilingStatus
{
    Draft = 1,
    Generated = 2,
    Uploaded = 3,
    Filed = 4,
    Accepted = 5,
    Rejected = 6
}

// Section 4A - B2B Invoices (Registered recipients)
public class Gstr1B2bInvoice
{
    public int Id { get; set; }
    public int Gstr1ReportId { get; set; }
    public Gstr1Report Report { get; set; } = null!;
    
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    
    // Recipient details
    public string Gstn { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal TaxableValue { get; set; }
    public decimal Cgst { get; set; }
    public decimal Sgst { get; set; }
    public decimal Igst { get; set; }
    public decimal Cess { get; set; }
    public decimal TotalValue { get; set; }
    
    public string PlaceOfSupply { get; set; } = string.Empty;  // State code
    public bool IsReverseCharge { get; set; }
}

// Section 5A - B2C Large Invoices (>2.5 Lakhs, inter-state)
public class Gstr1B2cLargeInvoice
{
    public int Id { get; set; }
    public int Gstr1ReportId { get; set; }
    public Gstr1Report Report { get; set; } = null!;
    
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal TaxableValue { get; set; }
    public decimal Cgst { get; set; }
    public decimal Sgst { get; set; }
    public decimal Igst { get; set; }
    public decimal Cess { get; set; }
    public decimal TotalValue { get; set; }
    
    public string PlaceOfSupply { get; set; } = string.Empty;
}

// Section 7 - B2C Small Invoices (Unregistered, <2.5 Lakhs)
public class Gstr1B2cSmallInvoice
{
    public int Id { get; set; }
    public int Gstr1ReportId { get; set; }
    public Gstr1Report Report { get; set; } = null!;
    
    // Aggregated by state
    public string PlaceOfSupply { get; set; } = string.Empty;  // State code
    public decimal TaxableValue { get; set; }
    public decimal Cgst { get; set; }
    public decimal Sgst { get; set; }
    public decimal Igst { get; set; }
    public decimal Cess { get; set; }
    public decimal TotalValue { get; set; }
    public int InvoiceCount { get; set; }
}

// Section 9A - Credit/Debit Notes
public class Gstr1CreditDebitNote
{
    public int Id { get; set; }
    public int Gstr1ReportId { get; set; }
    public Gstr1Report Report { get; set; } = null!;
    
    public int InvoiceId { get; set; }  // Original invoice
    public Invoice Invoice { get; set; } = null!;
    
    public string NoteType { get; set; } = "Credit";  // Credit or Debit
    public string NoteNumber { get; set; } = string.Empty;
    public DateTime NoteDate { get; set; }
    
    public string Gstn { get; set; } = string.Empty;
    public decimal TaxableValue { get; set; }
    public decimal Cgst { get; set; }
    public decimal Sgst { get; set; }
    public decimal Igst { get; set; }
    public decimal Cess { get; set; }
    public decimal TotalValue { get; set; }
}

// Section 6A - Export Invoices
public class Gstr1ExportInvoice
{
    public int Id { get; set; }
    public int Gstr1ReportId { get; set; }
    public Gstr1Report Report { get; set; } = null!;
    
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    
    public string PortCode { get; set; } = string.Empty;
    public string ShippingBillNumber { get; set; } = string.Empty;
    public DateTime? ShippingBillDate { get; set; }
    
    public decimal TaxableValue { get; set; }
    public decimal Igst { get; set; }
    public decimal Cess { get; set; }
    public decimal TotalValue { get; set; }
    
    public bool IsLutBond { get; set; }  // Letter of Undertaking
}

/// <summary>
/// GSTR-3B Summary Model - Monthly return for tax payment
/// Simplified summary for quick filing
/// </summary>
public class Gstr3bSummary
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    
    public string TaxPeriod { get; set; } = string.Empty;  // "MMYYYY"
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    // Part 3.1 - Outward supplies (auto-filled from GSTR-1)
    public decimal OutwardTaxableValue { get; set; }
    public decimal OutwardCgst { get; set; }
    public decimal OutwardSgst { get; set; }
    public decimal OutwardIgst { get; set; }
    public decimal OutwardCess { get; set; }
    
    // Part 4 - Eligible ITC
    public decimal ItcCgst { get; set; }
    public decimal ItcSgst { get; set; }
    public decimal ItcIgst { get; set; }
    public decimal ItcCess { get; set; }
    
    // Part 5 - Reverse charge liability
    public decimal RcmCgst { get; set; }
    public decimal RcmSgst { get; set; }
    public decimal RcmIgst { get; set; }
    
    // Payment calculation
    public decimal NetPayableCgst { get; set; }
    public decimal NetPayableSgst { get; set; }
    public decimal NetPayableIgst { get; set; }
    
    public GstrFilingStatus Status { get; set; } = GstrFilingStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
