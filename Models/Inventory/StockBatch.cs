namespace TaxAccount.Models.Inventory;

public class StockBatch
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ItemId { get; set; }
    public int GodownId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime MfgDate { get; set; }
    public DateTime ExpDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    
    // Navigation
    public Item? Item { get; set; }
    public Godown? Godown { get; set; }
}