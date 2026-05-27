using TaxAccount.Models.Accounting;
using TaxAccount.Models;
using Microsoft.EntityFrameworkCore;
using TaxAccount.Data;

namespace TaxAccount.Services;

public interface IAccountingService
{
    Task<List<AccountHead>> GetChartOfAccountsAsync(int tenantId);
    Task<AccountHead> CreateAccountAsync(AccountHead account);
    Task<AccountHead> UpdateAccountAsync(AccountHead account);
    Task DeleteAccountAsync(int id);
    
    Task<List<LedgerEntry>> GetGeneralLedgerAsync(int tenantId, int? accountHeadId = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<TrialBalanceDto> GetTrialBalanceAsync(int tenantId, DateTime fromDate, DateTime toDate);
    Task<FinancialStatementDto> GetBalanceSheetAsync(int tenantId, DateTime asOfDate);
    Task<FinancialStatementDto> GetProfitLossAsync(int tenantId, DateTime fromDate, DateTime toDate);
    
    Task<PostingResult> AutoPostSaleInvoiceAsync(Models.SaleInvoice invoice);
    Task<PostingResult> AutoPostPurchaseBillAsync(Models.PurchaseBill bill);
    Task<PostingResult> PostPaymentAsync(Models.Payment payment);
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
    public AccountType Type { get; set; }
    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal CurrentDebit { get; set; }
    public decimal CurrentCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
}

public class FinancialStatementDto
{
    public List<FinancialStatementItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class FinancialStatementItem
{
    public int AccountHeadId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal Amount { get; set; }
    public int Level { get; set; } // For hierarchy display
}

public class PostingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<int> LedgerEntryIds { get; set; } = new();
}
