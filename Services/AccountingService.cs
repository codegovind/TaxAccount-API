using Microsoft.EntityFrameworkCore;
using TaxAccount.Data;
using TaxAccount.DTOs.Accounting;
using TaxAccount.Models;
using TaxAccount.Models.Accounting;

namespace TaxAccount.Services;

public interface IAccountingService
{
    Task<List<AccountHeadDto>> GetChartOfAccountsAsync();
    Task<AccountHeadDto> CreateAccountHeadAsync(CreateAccountHeadDto dto);
    Task<LedgerEntryDto> PostTransactionAsync(LedgerEntryDto entry);
    Task<PostingResult> AutoPostSaleInvoiceAsync(Invoice invoice);
    Task<PostingResult> AutoPostPurchaseBillAsync(PurchaseBill bill);
    Task<List<TrialBalanceDto>> GetTrialBalanceAsync(DateTime fromDate, DateTime toDate);
    Task<List<FinancialStatementDto>> GetProfitLossAsync(DateTime fromDate, DateTime toDate);
    Task<List<FinancialStatementDto>> GetBalanceSheetAsync(DateTime asOfDate);
}

public class AccountingService : IAccountingService
{
    private readonly AppDbContext _context;
    private readonly ITenantService _tenantService;

    public AccountingService(AppDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<List<AccountHeadDto>> GetChartOfAccountsAsync()
    {
        var accounts = await _context.AccountHeads
            .Where(a => a.IsActive)
            .OrderBy(a => a.Code)
            .ToListAsync();

        // Build hierarchical structure
        var dtos = accounts.Select(a => new AccountHeadDto
        {
            Id = a.Id,
            Name = a.Name,
            Code = a.Code,
            Type = a.Type.ToString(),
            ParentId = a.ParentId,
            OpeningBalance = a.OpeningBalance,
            IsActive = a.IsActive,
            Children = new List<AccountHeadDto>()
        }).ToList();

        // Build tree
        var lookup = dtos.ToDictionary(x => x.Id);
        var roots = new List<AccountHeadDto>();

        foreach (var dto in dtos)
        {
            if (dto.ParentId.HasValue && lookup.ContainsKey(dto.ParentId.Value))
            {
                lookup[dto.ParentId.Value].Children.Add(dto);
            }
            else
            {
                roots.Add(dto);
            }
        }

        return roots;
    }

    public async Task<AccountHeadDto> CreateAccountHeadAsync(CreateAccountHeadDto dto)
    {
        var accountType = (AccountType)Enum.Parse(typeof(AccountType), dto.Type, true);
        
        var account = new AccountHead
        {
            Name = dto.Name,
            Code = dto.Code,
            Type = accountType,
            ParentId = dto.ParentId,
            OpeningBalance = dto.OpeningBalance,
            TenantId = _tenantService.GetTenantId()
        };

        _context.AccountHeads.Add(account);
        await _context.SaveChangesAsync();

        return new AccountHeadDto
        {
            Id = account.Id,
            Name = account.Name,
            Code = account.Code,
            Type = account.Type.ToString(),
            ParentId = account.ParentId,
            OpeningBalance = account.OpeningBalance,
            IsActive = account.IsActive
        };
    }

    public async Task<LedgerEntryDto> PostTransactionAsync(LedgerEntryDto dto)
    {
        var entry = new LedgerEntry
        {
            AccountHeadId = dto.AccountHeadId,
            Date = dto.Date,
            VoucherType = dto.VoucherType,
            VoucherId = dto.VoucherId,
            VoucherNumber = dto.VoucherNumber,
            Narration = dto.Narration,
            Debit = dto.Debit,
            Credit = dto.Credit,
            TenantId = _tenantService.GetTenantId(),
            CreatedByUserId = 1, // TODO: Get from current user
            CreatedAt = DateTime.UtcNow
        };

        _context.LedgerEntries.Add(entry);
        await _context.SaveChangesAsync();

        var accountHead = await _context.AccountHeads.FindAsync(entry.AccountHeadId);

        return new LedgerEntryDto
        {
            Id = entry.Id,
            AccountHeadId = entry.AccountHeadId,
            AccountHeadName = accountHead?.Name ?? string.Empty,
            Date = entry.Date,
            VoucherType = entry.VoucherType,
            VoucherId = entry.VoucherId,
            VoucherNumber = entry.VoucherNumber,
            Narration = entry.Narration,
            Debit = entry.Debit,
            Credit = entry.Credit,
            CreatedAt = entry.CreatedAt
        };
    }

    public async Task<PostingResult> AutoPostSaleInvoiceAsync(Invoice invoice)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var ledgerEntries = new List<int>();
            var tenantId = _tenantService.GetTenantId();
            
            // Find accounts (these should be configured in Chart of Accounts)
            var salesAccount = await _context.AccountHeads
                .FirstOrDefaultAsync(a => a.Type == AccountType.Income && a.TenantId == tenantId);
            
            var debtorAccount = await _context.AccountHeads
                .FirstOrDefaultAsync(a => a.Type == AccountType.Asset && a.Name.Contains("Debtors") && a.TenantId == tenantId);

            if (salesAccount == null || debtorAccount == null)
            {
                return new PostingResult 
                { 
                    Success = false, 
                    Message = "Required accounts not configured. Please set up Sales and Debtors accounts." 
                };
            }

            // 1. Debit: Debtor (Customer owes money)
            var debitEntry = new LedgerEntry
            {
                AccountHeadId = debtorAccount.Id,
                Date = invoice.InvoiceDate,
                VoucherType = "Sale",
                VoucherId = invoice.Id,
                VoucherNumber = invoice.InvoiceNumber,
                Narration = $"Sale Invoice {invoice.InvoiceNumber} - {invoice.Contact?.Name}",
                Debit = invoice.TotalAmount,
                Credit = 0,
                TenantId = tenantId,
                CreatedByUserId = invoice.CreatedByUserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.LedgerEntries.Add(debitEntry);
            ledgerEntries.Add(debitEntry.Id);

            // 2. Credit: Sales Account
            var creditEntry = new LedgerEntry
            {
                AccountHeadId = salesAccount.Id,
                Date = invoice.InvoiceDate,
                VoucherType = "Sale",
                VoucherId = invoice.Id,
                VoucherNumber = invoice.InvoiceNumber,
                Narration = $"Sale Invoice {invoice.InvoiceNumber} - {invoice.Contact?.Name}",
                Debit = 0,
                Credit = invoice.SubTotal,
                TenantId = tenantId,
                CreatedByUserId = invoice.CreatedByUserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.LedgerEntries.Add(creditEntry);
            ledgerEntries.Add(creditEntry.Id);

            // 3. Credit: Tax Accounts (CGST/SGST/IGST)
            if (invoice.TaxAmount > 0)
            {
                var taxAccount = await _context.AccountHeads
                    .FirstOrDefaultAsync(a => a.Type == AccountType.Liability && a.Name.Contains("Output Tax") && a.TenantId == tenantId);
                
                if (taxAccount != null)
                {
                    var taxEntry = new LedgerEntry
                    {
                        AccountHeadId = taxAccount.Id,
                        Date = invoice.InvoiceDate,
                        VoucherType = "Sale",
                        VoucherId = invoice.Id,
                        VoucherNumber = invoice.InvoiceNumber,
                        Narration = $"Tax on Sale Invoice {invoice.InvoiceNumber}",
                        Debit = 0,
                        Credit = invoice.TaxAmount,
                        TenantId = tenantId,
                        CreatedByUserId = invoice.CreatedByUserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.LedgerEntries.Add(taxEntry);
                    ledgerEntries.Add(taxEntry.Id);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new PostingResult 
            { 
                Success = true, 
                Message = "Invoice posted to ledger successfully",
                LedgerEntryIds = ledgerEntries
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new PostingResult 
            { 
                Success = false, 
                Message = $"Failed to post invoice: {ex.Message}" 
            };
        }
    }

    public async Task<PostingResult> AutoPostPurchaseBillAsync(PurchaseBill bill)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var ledgerEntries = new List<int>();
            var tenantId = _tenantService.GetTenantId();
            
            // Find accounts
            var purchaseAccount = await _context.AccountHeads
                .FirstOrDefaultAsync(a => a.Type == AccountType.Expense && a.TenantId == tenantId);
            
            var creditorAccount = await _context.AccountHeads
                .FirstOrDefaultAsync(a => a.Type == AccountType.Liability && a.Name.Contains("Creditors") && a.TenantId == tenantId);

            if (purchaseAccount == null || creditorAccount == null)
            {
                return new PostingResult 
                { 
                    Success = false, 
                    Message = "Required accounts not configured. Please set up Purchase and Creditors accounts." 
                };
            }

            // 1. Debit: Purchase Account
            var debitEntry = new LedgerEntry
            {
                AccountHeadId = purchaseAccount.Id,
                Date = bill.BillDate,
                VoucherType = "Purchase",
                VoucherId = bill.Id,
                VoucherNumber = bill.BillNumber,
                Narration = $"Purchase Bill {bill.BillNumber} - {bill.Contact?.Name}",
                Debit = bill.SubTotal,
                Credit = 0,
                TenantId = tenantId,
                CreatedByUserId = bill.CreatedByUserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.LedgerEntries.Add(debitEntry);
            ledgerEntries.Add(debitEntry.Id);

            // 2. Debit: Input Tax (if any)
            if (bill.TaxAmount > 0)
            {
                var taxAccount = await _context.AccountHeads
                    .FirstOrDefaultAsync(a => a.Type == AccountType.Liability && a.Name.Contains("Input Tax") && a.TenantId == tenantId);
                
                if (taxAccount != null)
                {
                    var taxEntry = new LedgerEntry
                    {
                        AccountHeadId = taxAccount.Id,
                        Date = bill.BillDate,
                        VoucherType = "Purchase",
                        VoucherId = bill.Id,
                        VoucherNumber = bill.BillNumber,
                        Narration = $"Input Tax on Purchase Bill {bill.BillNumber}",
                        Debit = bill.TaxAmount,
                        Credit = 0,
                        TenantId = tenantId,
                        CreatedByUserId = bill.CreatedByUserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.LedgerEntries.Add(taxEntry);
                    ledgerEntries.Add(taxEntry.Id);
                }
            }

            // 3. Credit: Creditor (We owe money to vendor)
            var creditEntry = new LedgerEntry
            {
                AccountHeadId = creditorAccount.Id,
                Date = bill.BillDate,
                VoucherType = "Purchase",
                VoucherId = bill.Id,
                VoucherNumber = bill.BillNumber,
                Narration = $"Purchase Bill {bill.BillNumber} - {bill.Contact?.Name}",
                Debit = 0,
                Credit = bill.TotalAmount,
                TenantId = tenantId,
                CreatedByUserId = bill.CreatedByUserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.LedgerEntries.Add(creditEntry);
            ledgerEntries.Add(creditEntry.Id);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new PostingResult 
            { 
                Success = true, 
                Message = "Purchase bill posted to ledger successfully",
                LedgerEntryIds = ledgerEntries
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new PostingResult 
            { 
                Success = false, 
                Message = $"Failed to post purchase bill: {ex.Message}" 
            };
        }
    }

    public async Task<List<TrialBalanceDto>> GetTrialBalanceAsync(DateTime fromDate, DateTime toDate)
    {
        var accounts = await _context.AccountHeads
            .Where(a => a.TenantId == _tenantService.GetTenantId())
            .OrderBy(a => a.Code)
            .ToListAsync();

        var trialBalance = new List<TrialBalanceDto>();

        foreach (var account in accounts)
        {
            var entries = await _context.LedgerEntries
                .Where(e => e.AccountHeadId == account.Id && 
                           e.Date >= fromDate && e.Date <= toDate &&
                           e.TenantId == _tenantService.GetTenantId())
                .ToListAsync();

            var totalDebit = entries.Sum(e => e.Debit);
            var totalCredit = entries.Sum(e => e.Credit);

            trialBalance.Add(new TrialBalanceDto
            {
                AccountCode = account.Code,
                AccountName = account.Name,
                OpeningDebit = account.OpeningBalance > 0 ? account.OpeningBalance : 0,
                OpeningCredit = account.OpeningBalance < 0 ? Math.Abs(account.OpeningBalance) : 0,
                CurrentDebit = totalDebit,
                CurrentCredit = totalCredit,
                ClosingDebit = (account.OpeningBalance + totalDebit - totalCredit) > 0 
                    ? account.OpeningBalance + totalDebit - totalCredit : 0,
                ClosingCredit = (account.OpeningBalance + totalDebit - totalCredit) < 0 
                    ? Math.Abs(account.OpeningBalance + totalDebit - totalCredit) : 0
            });
        }

        return trialBalance;
    }

    public async Task<List<FinancialStatementDto>> GetProfitLossAsync(DateTime fromDate, DateTime toDate)
    {
        var incomeAccounts = await _context.AccountHeads
            .Where(a => a.Type == AccountType.Income && a.TenantId == _tenantService.GetTenantId())
            .ToListAsync();

        var expenseAccounts = await _context.AccountHeads
            .Where(a => a.Type == AccountType.Expense && a.TenantId == _tenantService.GetTenantId())
            .ToListAsync();

        var result = new List<FinancialStatementDto>();

        // Income Section
        var incomeSection = new FinancialStatementDto 
        { 
            AccountName = "Income", 
            Level = 0,
            Amount = 0 
        };

        foreach (var account in incomeAccounts)
        {
            var amount = await _context.LedgerEntries
                .Where(e => e.AccountHeadId == account.Id && 
                           e.Date >= fromDate && e.Date <= toDate &&
                           e.TenantId == _tenantService.GetTenantId())
                .SumAsync(e => e.Credit - e.Debit);

            incomeSection.Children.Add(new FinancialStatementDto
            {
                AccountName = account.Name,
                Amount = amount,
                Level = 1
            });
            incomeSection.Amount += amount;
        }

        result.Add(incomeSection);

        // Expense Section
        var expenseSection = new FinancialStatementDto 
        { 
            AccountName = "Expenses", 
            Level = 0,
            Amount = 0 
        };

        foreach (var account in expenseAccounts)
        {
            var amount = await _context.LedgerEntries
                .Where(e => e.AccountHeadId == account.Id && 
                           e.Date >= fromDate && e.Date <= toDate &&
                           e.TenantId == _tenantService.GetTenantId())
                .SumAsync(e => e.Debit - e.Credit);

            expenseSection.Children.Add(new FinancialStatementDto
            {
                AccountName = account.Name,
                Amount = amount,
                Level = 1
            });
            expenseSection.Amount += amount;
        }

        result.Add(expenseSection);

        // Net Profit/Loss
        var netProfit = incomeSection.Amount - expenseSection.Amount;
        result.Add(new FinancialStatementDto
        {
            AccountName = netProfit >= 0 ? "Net Profit" : "Net Loss",
            Amount = Math.Abs(netProfit),
            Level = 0
        });

        return result;
    }

    public async Task<List<FinancialStatementDto>> GetBalanceSheetAsync(DateTime asOfDate)
    {
        var assetAccounts = await _context.AccountHeads
            .Where(a => a.Type == AccountType.Asset && a.TenantId == _tenantService.GetTenantId())
            .ToListAsync();

        var liabilityAccounts = await _context.AccountHeads
            .Where(a => a.Type == AccountType.Liability && a.TenantId == _tenantService.GetTenantId())
            .ToListAsync();

        var equityAccounts = await _context.AccountHeads
            .Where(a => a.Type == AccountType.Equity && a.TenantId == _tenantService.GetTenantId())
            .ToListAsync();

        var result = new List<FinancialStatementDto>();

        // Assets Section
        var assetsSection = new FinancialStatementDto 
        { 
            AccountName = "Assets", 
            Level = 0,
            Amount = 0 
        };

        foreach (var account in assetAccounts)
        {
            var entries = await _context.LedgerEntries
                .Where(e => e.AccountHeadId == account.Id && 
                           e.Date <= asOfDate &&
                           e.TenantId == _tenantService.GetTenantId())
                .ToListAsync();

            var balance = account.OpeningBalance + entries.Sum(e => e.Debit - e.Credit);
            
            assetsSection.Children.Add(new FinancialStatementDto
            {
                AccountName = account.Name,
                Amount = balance,
                Level = 1
            });
            assetsSection.Amount += balance;
        }

        result.Add(assetsSection);

        // Liabilities Section
        var liabilitiesSection = new FinancialStatementDto 
        { 
            AccountName = "Liabilities", 
            Level = 0,
            Amount = 0 
        };

        foreach (var account in liabilityAccounts)
        {
            var entries = await _context.LedgerEntries
                .Where(e => e.AccountHeadId == account.Id && 
                           e.Date <= asOfDate &&
                           e.TenantId == _tenantService.GetTenantId())
                .ToListAsync();

            var balance = account.OpeningBalance + entries.Sum(e => e.Credit - e.Debit);
            
            liabilitiesSection.Children.Add(new FinancialStatementDto
            {
                AccountName = account.Name,
                Amount = balance,
                Level = 1
            });
            liabilitiesSection.Amount += balance;
        }

        result.Add(liabilitiesSection);

        // Equity Section
        var equitySection = new FinancialStatementDto 
        { 
            AccountName = "Equity", 
            Level = 0,
            Amount = 0 
        };

        foreach (var account in equityAccounts)
        {
            var entries = await _context.LedgerEntries
                .Where(e => e.AccountHeadId == account.Id && 
                           e.Date <= asOfDate &&
                           e.TenantId == _tenantService.GetTenantId())
                .ToListAsync();

            var balance = account.OpeningBalance + entries.Sum(e => e.Credit - e.Debit);
            
            equitySection.Children.Add(new FinancialStatementDto
            {
                AccountName = account.Name,
                Amount = balance,
                Level = 1
            });
            equitySection.Amount += balance;
        }

        result.Add(equitySection);

        return result;
    }
}
