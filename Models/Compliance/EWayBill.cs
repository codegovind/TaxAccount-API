namespace TaxAccount.Models.Compliance;

public class EWayBill
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string EWayBillNumber { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public DateTime ValidUntil { get; set; }
    public string Irn { get; set; } = string.Empty;
    public string JsonData { get; set; } = string.Empty;
    public int TenantId { get; set; }
}
