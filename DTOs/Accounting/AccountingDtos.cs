namespace TaxAccount.DTOs.Accounting;

public class AccountHeadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Asset, Liability, etc.
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public List<AccountHeadDto> Children { get; set; } = new();
}

public class CreateAccountHeadDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public decimal OpeningBalance { get; set; }
}

public class LedgerEntryDto
{
    public int Id { get; set; }
    public int AccountHeadId { get; set; }
    public string AccountHeadName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string VoucherType { get; set; } = string.Empty;
    public int? VoucherId { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TrialBalanceDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal CurrentDebit { get; set; }
    public decimal CurrentCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
}

public class FinancialStatementDto
{
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Level { get; set; } // For hierarchy display
    public List<FinancialStatementDto> Children { get; set; } = new();
}

public class PostingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<int> LedgerEntryIds { get; set; } = new();
}
