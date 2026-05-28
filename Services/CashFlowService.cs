using TaxAccount.Data;
using TaxAccount.Models;
using TaxAccount.Models.Accounting;
using TaxAccount.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace TaxAccount.Services;

public class CashFlowService
{
    private readonly AppDbContext _context;

    public CashFlowService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CashFlowStatementDto> CalculateDirectMethodAsync(DateTime fromDate, DateTime toDate, int tenantId)
    {
        var result = new CashFlowStatementDto
        {
            Period = $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}",
            OpeningBalance = await GetOpeningBalanceAsync(fromDate, tenantId),
            ClosingBalance = await GetClosingBalanceAsync(toDate, tenantId)
        };

        // Get all Cash/Bank Account IDs
        var cashBankIds = await _context.AccountHeads
            .Where(a => a.TenantId == tenantId && (a.Name.Contains("Cash") || a.Name.Contains("Bank")))
            .Select(a => a.Id)
            .ToListAsync();

        // Fetch entries where Cash/Bank is involved
        var entries = await _context.VoucherEntries
            .Include(e => e.AccountHead)
            .Where(e => e.TenantId == tenantId && 
                        e.Date >= fromDate && 
                        e.Date <= toDate &&
                        cashBankIds.Contains(e.AccountHeadId))
            .ToListAsync();

        // Operating Activities
        result.OperatingActivities.Name = "Cash Flow from Operating Activities";
        result.OperatingActivities.Items = MapOperatingActivities(entries, cashBankIds);
        result.OperatingActivities.NetAmount = result.OperatingActivities.Items.Sum(i => i.IsIncome ? i.Amount : -i.Amount);

        // Investing Activities
        result.InvestingActivities.Name = "Cash Flow from Investing Activities";
        result.InvestingActivities.Items = MapInvestingActivities(entries, cashBankIds);
        result.InvestingActivities.NetAmount = result.InvestingActivities.Items.Sum(i => i.IsIncome ? i.Amount : -i.Amount);

        // Financing Activities
        result.FinancingActivities.Name = "Cash Flow from Financing Activities";
        result.FinancingActivities.Items = MapFinancingActivities(entries, cashBankIds);
        result.FinancingActivities.NetAmount = result.FinancingActivities.Items.Sum(i => i.IsIncome ? i.Amount : -i.Amount);

        result.NetChange = result.OperatingActivities.NetAmount + 
                           result.InvestingActivities.NetAmount + 
                           result.FinancingActivities.NetAmount;

        return result;
    }

    public async Task<CashFlowStatementDto> CalculateIndirectMethodAsync(DateTime fromDate, DateTime toDate, int tenantId)
    {
        // For now, returns Direct Method structure. 
        // Real implementation would start with Net Profit and adjust.
        return await CalculateDirectMethodAsync(fromDate, toDate, tenantId);
    }

    public async Task<List<TransactionDetailDto>> GetTransactionDetailsAsync(int accountHeadId, DateTime fromDate, DateTime toDate, int tenantId)
    {
        var entries = await _context.VoucherEntries
            .Include(e => e.AccountHead)
            .Where(e => e.TenantId == tenantId && 
                       e.Date >= fromDate && 
                       e.Date <= toDate && 
                       e.AccountHeadId == accountHeadId)
            .OrderBy(e => e.Date)
            .ToListAsync();

        return entries.Select(e => new TransactionDetailDto
        {
            Date = e.Date,
            VoucherNumber = e.VoucherNumber ?? string.Empty,
            PartyName = e.AccountHead?.Name ?? string.Empty,
            Narration = e.Narration ?? string.Empty,
            Debit = e.Debit,
            Credit = e.Credit
        }).ToList();
    }

    private async Task<decimal> GetOpeningBalanceAsync(DateTime fromDate, int tenantId)
    {
        var cashBankNames = new[] { "Cash", "Bank" };
        var accountIds = await _context.AccountHeads
            .Where(a => a.TenantId == tenantId && cashBankNames.Any(n => a.Name.Contains(n)))
            .Select(a => a.Id)
            .ToListAsync();

        if (!accountIds.Any()) return 0;

        // Balance = Sum(Debit) - Sum(Credit)
        var opening = await _context.VoucherEntries
            .Where(e => e.TenantId == tenantId && 
                       e.Date < fromDate && 
                       accountIds.Contains(e.AccountHeadId))
            .SumAsync(e => e.Debit - e.Credit);

        return opening;
    }

    private async Task<decimal> GetClosingBalanceAsync(DateTime toDate, int tenantId)
    {
        var cashBankNames = new[] { "Cash", "Bank" };
        var accountIds = await _context.AccountHeads
            .Where(a => a.TenantId == tenantId && cashBankNames.Any(n => a.Name.Contains(n)))
            .Select(a => a.Id)
            .ToListAsync();

        if (!accountIds.Any()) return 0;

        var closing = await _context.VoucherEntries
            .Where(e => e.TenantId == tenantId && 
                       e.Date <= toDate && 
                       accountIds.Contains(e.AccountHeadId))
            .SumAsync(e => e.Debit - e.Credit);

        return closing;
    }

    private List<LineItemDto> MapOperatingActivities(List<VoucherEntry> entries, List<int> cashBankIds)
    {
        var items = new List<LineItemDto>();
        
        // Filter out contra transactions (Cash <-> Bank)
        var nonContraEntries = entries.Where(e => !cashBankIds.Contains(e.AccountHeadId)).ToList();

        var grouped = nonContraEntries.GroupBy(e => e.AccountHead?.Name ?? "Unknown");
        
        foreach (var group in grouped)
        {
            // Logic: If Cash/Bank was Debited, it's an Inflow (Income/Asset Increase)
            // If Cash/Bank was Credited, it's an Outflow (Expense/Asset Decrease)
            // Since we filtered Cash/Bank IDs, 'e' here is the counter-party.
            // We need to look at the original entry to see if Cash was Dr or Cr.
            
            // Simplified: Summing net impact on cash for this party
            // Note: In a double entry system, if Cash is Dr, CounterParty is Cr.
            // We need to re-fetch or calculate based on the Cash side of the transaction.
            // For this simplified version, we assume the list contains the Cash/Bank entries.
            // If the entry IS a cash entry:
            
            decimal netCashFlow = group.Sum(e => e.Debit - e.Credit);

            if (netCashFlow != 0)
            {
                items.Add(new LineItemDto
                {
                    Description = group.Key,
                    Amount = Math.Abs(netCashFlow),
                    IsIncome = netCashFlow > 0 // Positive means Cash came in
                });
            }
        }
        return items;
    }

    private List<LineItemDto> MapInvestingActivities(List<VoucherEntry> entries, List<int> cashBankIds)
    {
        // Placeholder: Filter for Asset accounts similarly
        return new List<LineItemDto>();
    }

    private List<LineItemDto> MapFinancingActivities(List<VoucherEntry> entries, List<int> cashBankIds)
    {
        // Placeholder: Filter for Capital/Loan accounts similarly
        return new List<LineItemDto>();
    }
}