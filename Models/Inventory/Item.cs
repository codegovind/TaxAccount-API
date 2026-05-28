namespace TaxAccount.Models.Inventory;

public class Item
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HsnCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public int GodownId { get; set; }
    
    // Navigation
    public Godown? Godown { get; set; }
    public ICollection<StockBatch> StockBatches { get; set; } = new List<StockBatch>();
}