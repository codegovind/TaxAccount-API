namespace TaxAccount.Models.Inventory;

public class Godown
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    
    // Navigation
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public ICollection<StockBatch> StockBatches { get; set; } = new List<StockBatch>();
}