namespace TaxAccount.DTOs.Accounting
{
    public class LedgerEntryDto
    {
        public int Id { get; set; }
        public int AccountHeadId { get; set; }
        public DateTime Date { get; set; }
        public string VoucherType { get; set; } = string.Empty;
        public int VoucherId { get; set; }
        public string VoucherNumber { get; set; } = string.Empty;
        public string Narration { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TrialBalanceDto
    {
        public List<TrialBalanceItem> Items { get; set; } = new();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
    }

    public class TrialBalanceItem
    {
        public int AccountHeadId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal CurrentDebit { get; set; }
        public decimal CurrentCredit { get; set; }
        public decimal ClosingDebit { get; set; }
        public decimal ClosingCredit { get; set; }
    }

    public class FinancialStatementDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<FinancialStatementItem> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    public class FinancialStatementItem
    {
        public int AccountHeadId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Level { get; set; }
    }

    public class PostingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<int> LedgerEntryIds { get; set; } = new();
    }
}
