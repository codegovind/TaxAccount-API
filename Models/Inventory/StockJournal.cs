namespace TaxAccount.Models.Inventory;

/// <summary>
/// Stock Journal - For manufacturing processes, godown transfers, and stock adjustments without financial impact
/// Similar to Tally's Stock Journal voucher (Alt+F7)
/// </summary>
public class StockJournal
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    // Type of stock journal
    public StockJournalType JournalType { get; set; } = StockJournalType.Manufacturing;
    
    // Source details (items going out)
    public int? SourceGodownId { get; set; }
    public Godown? SourceGodown { get; set; }
    
    // Destination details (items coming in)
    public int? DestinationGodownId { get; set; }
    public Godown? DestinationGodown { get; set; }
    
    // For manufacturing: Raw materials consumed
    public ICollection<StockJournalItem> ConsumedItems { get; set; } = new List<StockJournalItem>();
    
    // For manufacturing: Finished goods produced
    public ICollection<StockJournalItem> ProducedItems { get; set; } = new List<StockJournalItem>();
    
    // For godown transfer: Items being transferred
    public ICollection<StockJournalItem> TransferItems { get; set; } = new List<StockJournalItem>();
    
    public string Narration { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum StockJournalType
{
    Manufacturing = 1,      // Raw material → Finished goods
    GodownTransfer = 2,     // Move stock between godowns
    MaterialIssue = 3,      // Issue material for production
    MaterialReceipt = 4,    // Receive finished goods from production
    ScrapAdjustment = 5     // Adjust scrap/wastage
}

public class StockJournalItem
{
    public int Id { get; set; }
    public int StockJournalId { get; set; }
    public StockJournal StockJournal { get; set; } = null!;
    
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;
    
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    
    // Rate for valuation (optional, uses avg cost if not provided)
    public decimal? Rate { get; set; }
    public decimal Amount { get; set; }
    
    // Batch tracking
    public int? StockBatchId { get; set; }
    public StockBatch? StockBatch { get; set; }
    
    // For manufacturing: BOM (Bill of Materials) reference
    public bool IsConsumed { get; set; }  // true = raw material, false = finished good
}
