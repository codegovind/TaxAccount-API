namespace TaxAccount.Models.Dtos;

public class CashFlowStatementDto
{
    public string Period { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal NetChange { get; set; }
    
    public ActivitySectionDto OperatingActivities { get; set; } = new();
    public ActivitySectionDto InvestingActivities { get; set; } = new();
    public ActivitySectionDto FinancingActivities { get; set; } = new();
}

public class ActivitySectionDto
{
    public string Name { get; set; } = string.Empty;
    public decimal NetAmount { get; set; }
    public List<LineItemDto> Items { get; set; } = new();
}

public class LineItemDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsIncome { get; set; } // True for Inflow, False for Outflow
}

public class TransactionDetailDto
{
    public DateTime Date { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}
