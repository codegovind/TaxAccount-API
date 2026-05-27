using TaxAccount.Models.Accounting;
using TaxAccount.Models;
using Microsoft.EntityFrameworkCore;
using TaxAccount.Data;

namespace TaxAccount.Services;

public class AccountingService : IAccountingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AccountingService> _logger;

    public AccountingService(AppDbContext context, ILogger<AccountingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<AccountHead>> GetChartOfAccountsAsync(int tenantId)
    {
        return await _context.AccountHeads
            .Where(h => h.TenantId == tenantId && h.IsActive)
            .OrderBy(h => h.Code)
            .ToListAsync();
    }

    public async Task<AccountHead> CreateAccountAsync(AccountHead account)
    {
        _context.AccountHeads.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<AccountHead> UpdateAccountAsync(AccountHead account)
    {
        var existing = await _context.AccountHeads.FindAsync(account.Id);
        if (existing == null) throw new Exception("Account not found");
        
        existing.Name = account.Name;
        existing.Code = account.Code;
        existing.Type = account.Type;
        existing.ParentId = account.ParentId;
        existing.OpeningBalance = account.OpeningBalance;
        
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAccountAsync(int id)
    {
        var account = await _context.AccountHeads.FindAsync(id);
        if (account != null)
        {
            account.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<LedgerEntry>> GetGeneralLedgerAsync(int tenantId, int? accountHeadId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.LedgerEntries
            .Include(e => e.AccountHead)
            .Where(e => e.TenantId == tenantId)
            .AsQueryable();

        if (accountHeadId.HasValue)
            query = query.Where(e => e.AccountHeadId == accountHeadId.Value);
        
        if (fromDate.HasValue)
            query = query.Where(e => e.Date >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(e => e.Date <= toDate.Value);

        return await query.OrderBy(e => e.Date).ThenBy(e => e.CreatedAt).ToListAsync();
    }

    public async Task<TrialBalanceDto> GetTrialBalanceAsync(int tenantId, DateTime fromDate, DateTime toDate)
    {
        var accounts = await _context.AccountHeads
            .Where(h => h.TenantId == tenantId && h.IsActive)
            .ToListAsync();

        var result = new TrialBalanceDto();
        
        foreach (var account in accounts)
        {
            var entries = await _context.LedgerEntries
                .Where(e => e.AccountHeadId == account.Id && e.TenantId == tenantId)
                .ToListAsync();

            var openingEntries = entries.Where(e => e.Date < fromDate).ToList();
            var currentEntries = entries.Where(e => e.Date >= fromDate && e.Date <= toDate).ToList();

            var item = new TrialBalanceItem
            {
                AccountHeadId = account.Id,
                AccountName = account.Name,
                AccountCode = account.Code,
                Type = account.Type,
                OpeningDebit = openingEntries.Sum(e => e.Debit),
                OpeningCredit = openingEntries.Sum(e => e.Credit),
                CurrentDebit = currentEntries.Sum(e => e.Debit),
                CurrentCredit = currentEntries.Sum(e => e.Credit)
            };

            var openingBalance = account.OpeningBalance;
            if (account.Type == AccountType.Asset || account.Type == AccountType.Expense)
            {
                item.OpeningDebit += openingBalance;
            }
            else
            {
                item.OpeningCredit += openingBalance;
            }

            item.ClosingDebit = item.OpeningDebit + item.CurrentDebit;
            item.ClosingCredit = item.OpeningCredit + item.CurrentCredit;

            if (item.ClosingDebit != 0 || item.ClosingCredit != 0)
                result.Items.Add(item);
        }

        result.TotalDebit = result.Items.Sum(i => i.ClosingDebit);
        result.TotalCredit = result.Items.Sum(i => i.ClosingCredit);
        
        return result;
    }

    public async Task<FinancialStatementDto> GetBalanceSheetAsync(int tenantId, DateTime asOfDate)
    {
        var accounts = await _context.AccountHeads
            .Where(h => h.TenantId == tenantId && h.IsActive && 
                       (h.Type == AccountType.Asset || h.Type == AccountType.Liability || h.Type == AccountType.Equity))
            .ToListAsync();

        var result = new FinancialStatementDto
        {
            FromDate = asOfDate,
            ToDate = asOfDate
        };

        foreach (var account in accounts)
        {
            var entries = await _context.LedgerEntries
                .Where(e => e.AccountHeadId == account.Id && e.TenantId == tenantId && e.Date <= asOfDate)
                .ToListAsync();

            var debitTotal = entries.Sum(e => e.Debit);
            var creditTotal = entries.Sum(e => e.Credit);
            
            decimal balance;
            if (account.Type == AccountType.Asset)
                balance = (account.OpeningBalance + debitTotal) - creditTotal;
            else
                balance = (account.OpeningBalance + creditTotal) - debitTotal;

            if (balance != 0)
            {
                result.Items.Add(new FinancialStatementItem
                {
                    AccountHeadId = account.Id,
                    AccountName = account.Name,
                    AccountCode = account.Code,
                    Type = account.Type,
                    Amount = Math.Abs(balance),
                    Level = account.ParentId.HasValue ? 1 : 0
                });
            }
        }

        result.TotalAmount = result.Items.Sum(i => i.Amount);
        return result;
    }

    public async Task<FinancialStatementDto> GetProfitLossAsync(int tenantId, DateTime fromDate, DateTime toDate)
    {
        var accounts = await _context.AccountHeads
            .Where(h => h.TenantId == tenantId && h.IsActive && 
                       (h.Type == AccountType.Income || h.Type == AccountType.Expense))
            .ToListAsync();

        var result = new FinancialStatementDto
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        foreach (var account in accounts)
        {
            var entries = await _context.LedgerEntries
                .Where(e => e.AccountHeadId == account.Id && e.TenantId == tenantId && 
                           e.Date >= fromDate && e.Date <= toDate)
                .ToListAsync();

            var debitTotal = entries.Sum(e => e.Debit);
            var creditTotal = entries.Sum(e => e.Credit);
            
            decimal balance;
            if (account.Type == AccountType.Expense)
                balance = debitTotal - creditTotal;
            else
                balance = creditTotal - debitTotal;

            if (balance != 0)
            {
                result.Items.Add(new FinancialStatementItem
                {
                    AccountHeadId = account.Id,
                    AccountName = account.Name,
                    AccountCode = account.Code,
                    Type = account.Type,
                    Amount = Math.Abs(balance),
                    Level = account.ParentId.HasValue ? 1 : 0
                });
            }
        }

        result.TotalAmount = result.Items.Sum(i => i.Amount);
        return result;
    }

    public async Task<PostingResult> AutoPostSaleInvoiceAsync(Models.SaleInvoice invoice)
    {
        try
        {
            var ledgerEntries = new List<LedgerEntry>();
            
            // Find Sales Account (assuming code starts with "4" for Income)
            var salesAccount = await _context.AccountHeads
                .FirstOrDefaultAsync(h => h.Code.StartsWith("4") && h.Type == AccountType.Income && h.TenantId == invoice.TenantId);
            
            if (salesAccount == null)
                return new PostingResult { Success = false, Message = "Sales account not found. Please create a Sales account in Chart of Accounts." };

            // Find Debtor/Customer account type
            var debtorGroup = await _context.AccountHeads
                .FirstOrDefaultAsync(h => h.Code.StartsWith("1") && h.Type == AccountType.Asset && h.TenantId == invoice.TenantId);

            // Debit: Debtor (Customer)
            ledgerEntries.Add(new LedgerEntry
            {
                AccountHeadId = debtorGroup?.Id ?? 1, // Fallback to first asset account
                Date = invoice.InvoiceDate,
                VoucherType = "Sale",
                VoucherId = invoice.Id,
                VoucherNumber = invoice.InvoiceNumber,
                Narration = $"Sale Invoice {invoice.InvoiceNumber} - {invoice.Contact?.Name}",
                Debit = invoice.TotalAmount,
                Credit = 0,
                TenantId = invoice.TenantId,
                CreatedByUserId = invoice.CreatedByUserId
            });

            // Credit: Sales Account
            ledgerEntries.Add(new LedgerEntry
            {
                AccountHeadId = salesAccount.Id,
                Date = invoice.InvoiceDate,
                VoucherType = "Sale",
                VoucherId = invoice.Id,
                VoucherNumber = invoice.InvoiceNumber,
                Narration = $"Sale Invoice {invoice.InvoiceNumber}",
                Debit = 0,
                Credit = invoice.SubTotal,
                TenantId = invoice.TenantId,
                CreatedByUserId = invoice.CreatedByUserId
            });

            // Credit: Tax (CGST/SGST/IGST)
            if (invoice.TaxAmount > 0)
            {
                var taxAccount = await _context.AccountHeads
                    .FirstOrDefaultAsync(h => h.Name.Contains("Output Tax") && h.TenantId == invoice.TenantId);
                
                if (taxAccount != null)
                {
                    ledgerEntries.Add(new LedgerEntry
                    {
                        AccountHeadId = taxAccount.Id,
                        Date = invoice.InvoiceDate,
                        VoucherType = "Sale",
                        VoucherId = invoice.Id,
                        VoucherNumber = invoice.InvoiceNumber,
                        Narration = $"Tax on Sale Invoice {invoice.InvoiceNumber}",
                        Debit = 0,
                        Credit = invoice.TaxAmount,
                        TenantId = invoice.TenantId,
                        CreatedByUserId = invoice.CreatedByUserId
                    });
                }
            }

            _context.LedgerEntries.AddRange(ledgerEntries);
            await _context.SaveChangesAsync();

            return new PostingResult
            {
                Success = true,
                Message = "Sale invoice posted to ledger successfully",
                LedgerEntryIds = ledgerEntries.Select(e => e.Id).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting sale invoice to ledger");
            return new PostingResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<PostingResult> AutoPostPurchaseBillAsync(Models.PurchaseBill bill)
    {
        try
        {
            var ledgerEntries = new List<LedgerEntry>();
            
            // Find Purchase Account (assuming code starts with "5" for Expense)
            var purchaseAccount = await _context.AccountHeads
                .FirstOrDefaultAsync(h => h.Code.StartsWith("5") && h.Type == AccountType.Expense && h.TenantId == bill.TenantId);
            
            if (purchaseAccount == null)
                return new PostingResult { Success = false, Message = "Purchase account not found. Please create a Purchase account in Chart of Accounts." };

            // Credit: Creditor (Vendor)
            var creditorGroup = await _context.AccountHeads
                .FirstOrDefaultAsync(h => h.Code.StartsWith("2") && h.Type == AccountType.Liability && h.TenantId == bill.TenantId);

            ledgerEntries.Add(new LedgerEntry
            {
                AccountHeadId = creditorGroup?.Id ?? 1,
                Date = bill.BillDate,
                VoucherType = "Purchase",
                VoucherId = bill.Id,
                VoucherNumber = bill.BillNumber,
                Narration = $"Purchase Bill {bill.BillNumber} - {bill.Contact?.Name}",
                Debit = bill.TotalAmount,
                Credit = 0,
                TenantId = bill.TenantId,
                CreatedByUserId = bill.CreatedByUserId
            });

            // Credit: Purchase Account
            ledgerEntries.Add(new LedgerEntry
            {
                AccountHeadId = purchaseAccount.Id,
                Date = bill.BillDate,
                VoucherType = "Purchase",
                VoucherId = bill.Id,
                VoucherNumber = bill.BillNumber,
                Narration = $"Purchase Bill {bill.BillNumber}",
                Debit = 0,
                Credit = bill.SubTotal,
                TenantId = bill.TenantId,
                CreatedByUserId = bill.CreatedByUserId
            });

            // Debit: Input Tax
            if (bill.TaxAmount > 0)
            {
                var taxAccount = await _context.AccountHeads
                    .FirstOrDefaultAsync(h => h.Name.Contains("Input Tax") && h.TenantId == bill.TenantId);
                
                if (taxAccount != null)
                {
                    ledgerEntries.Add(new LedgerEntry
                    {
                        AccountHeadId = taxAccount.Id,
                        Date = bill.BillDate,
                        VoucherType = "Purchase",
                        VoucherId = bill.Id,
                        VoucherNumber = bill.BillNumber,
                        Narration = $"Input Tax on Purchase Bill {bill.BillNumber}",
                        Debit = bill.TaxAmount,
                        Credit = 0,
                        TenantId = bill.TenantId,
                        CreatedByUserId = bill.CreatedByUserId
                    });
                }
            }

            _context.LedgerEntries.AddRange(ledgerEntries);
            await _context.SaveChangesAsync();

            return new PostingResult
            {
                Success = true,
                Message = "Purchase bill posted to ledger successfully",
                LedgerEntryIds = ledgerEntries.Select(e => e.Id).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting purchase bill to ledger");
            return new PostingResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<PostingResult> PostPaymentAsync(Models.Payment payment)
    {
        try
        {
            var ledgerEntries = new List<LedgerEntry>();
            
            if (payment.VoucherType == VoucherType.Payment)
            {
                // Payment to vendor
                // Credit: Bank/Cash
                // Debit: Creditor
                
                var bankAccount = await _context.AccountHeads
                    .FirstOrDefaultAsync(h => h.Name.Contains("Bank") || h.Name.Contains("Cash"));
                
                if (bankAccount == null)
                    return new PostingResult { Success = false, Message = "Bank or Cash account not found" };

                ledgerEntries.Add(new LedgerEntry
                {
                    AccountHeadId = bankAccount.Id,
                    Date = payment.Date,
                    VoucherType = "Payment",
                    VoucherId = payment.Id,
                    VoucherNumber = payment.VoucherNumber,
                    Narration = payment.Narration,
                    Debit = 0,
                    Credit = payment.Amount,
                    TenantId = payment.TenantId,
                    CreatedByUserId = payment.CreatedByUserId
                });

                ledgerEntries.Add(new LedgerEntry
                {
                    AccountHeadId = payment.ContactId, // Assuming contact linked to creditor account
                    Date = payment.Date,
                    VoucherType = "Payment",
                    VoucherId = payment.Id,
                    VoucherNumber = payment.VoucherNumber,
                    Narration = payment.Narration,
                    Debit = payment.Amount,
                    Credit = 0,
                    TenantId = payment.TenantId,
                    CreatedByUserId = payment.CreatedByUserId
                });
            }
            else if (payment.VoucherType == VoucherType.Receipt)
            {
                // Receipt from customer
                // Debit: Bank/Cash
                // Credit: Debtor
                
                var bankAccount = await _context.AccountHeads
                    .FirstOrDefaultAsync(h => h.Name.Contains("Bank") || h.Name.Contains("Cash"));
                
                if (bankAccount == null)
                    return new PostingResult { Success = false, Message = "Bank or Cash account not found" };

                ledgerEntries.Add(new LedgerEntry
                {
                    AccountHeadId = bankAccount.Id,
                    Date = payment.Date,
                    VoucherType = "Receipt",
                    VoucherId = payment.Id,
                    VoucherNumber = payment.VoucherNumber,
                    Narration = payment.Narration,
                    Debit = payment.Amount,
                    Credit = 0,
                    TenantId = payment.TenantId,
                    CreatedByUserId = payment.CreatedByUserId
                });

                ledgerEntries.Add(new LedgerEntry
                {
                    AccountHeadId = payment.ContactId,
                    Date = payment.Date,
                    VoucherType = "Receipt",
                    VoucherId = payment.Id,
                    VoucherNumber = payment.VoucherNumber,
                    Narration = payment.Narration,
                    Debit = 0,
                    Credit = payment.Amount,
                    TenantId = payment.TenantId,
                    CreatedByUserId = payment.CreatedByUserId
                });
            }

            _context.LedgerEntries.AddRange(ledgerEntries);
            await _context.SaveChangesAsync();

            return new PostingResult
            {
                Success = true,
                Message = "Payment posted successfully",
                LedgerEntryIds = ledgerEntries.Select(e => e.Id).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting payment");
            return new PostingResult { Success = false, Message = ex.Message };
        }
    }
}
