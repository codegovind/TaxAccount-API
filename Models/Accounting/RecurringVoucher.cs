namespace TaxAccount.Models.Accounting;

/// <summary>
/// Recurring Voucher Template - For automated periodic entries (rent, salary, loan EMI, etc.)
/// Similar to Tally's Recurring Vouchers feature
/// </summary>
public class RecurringVoucher
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    
    // Base voucher type
    public VoucherType VoucherType { get; set; } = VoucherType.Journal;
    
    // Frequency settings
    public RecurringFrequency Frequency { get; set; } = RecurringFrequency.Monthly;
    public int Interval { get; set; } = 1;  // Every N weeks/months/years
    
    // Scheduling
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }  // e.g., "Every Monday"
    public int DayOfMonth { get; set; } = 1;   // e.g., "1st of every month"
    
    // Voucher defaults
    public decimal FixedAmount { get; set; }
    public int? DefaultContactId { get; set; }
    public Contact? DefaultContact { get; set; }
    
    // Account entries (template for each occurrence)
    public ICollection<RecurringVoucherEntry> Entries { get; set; } = new List<RecurringVoucherEntry>();
    
    // Tracking
    public DateTime? LastGeneratedDate { get; set; }
    public int TotalGenerated { get; set; }
    public bool IsActive { get; set; } = true;
    
    public string Narration { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum RecurringFrequency
{
    Daily = 1,
    Weekly = 2,
    BiWeekly = 3,
    Monthly = 4,
    Quarterly = 5,
    HalfYearly = 6,
    Yearly = 7
}

public class RecurringVoucherEntry
{
    public int Id { get; set; }
    public int RecurringVoucherId { get; set; }
    public RecurringVoucher RecurringVoucher { get; set; } = null!;
    
    public int AccountHeadId { get; set; }
    public AccountHead AccountHead { get; set; } = null!;
    
    // Debit or Credit amount (one will be zero)
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    
    public string Narration { get; set; } = string.Empty;
}

/// <summary>
/// Log of generated vouchers from recurring templates
/// </summary>
public class RecurringVoucherLog
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int RecurringVoucherId { get; set; }
    public RecurringVoucher RecurringVoucher { get; set; } = null!;
    
    // Reference to the actual voucher generated
    public int? GeneratedVoucherEntryId { get; set; }
    public VoucherEntry? GeneratedVoucherEntry { get; set; }
    
    public DateTime ScheduledDate { get; set; }
    public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    
    public RecurringVoucherLogStatus Status { get; set; } = RecurringVoucherLogStatus.Pending;
    public string? ErrorMessage { get; set; }
}

public enum RecurringVoucherLogStatus
{
    Pending = 1,
    Generated = 2,
    Failed = 3,
    Skipped = 4  // e.g., holiday, weekend
}
