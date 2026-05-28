// using TaxAccount.Data;
// using TaxAccount.Models.Accounting;
// using TaxAccount.Models.Inventory;
// using Microsoft.EntityFrameworkCore;

// namespace TaxAccount.Services;

// public class DbSeeder
// {
//     private readonly AppDbContext _context;
//     private readonly ILogger<DbSeeder> _logger;

//     public DbSeeder(AppDbContext context, ILogger<DbSeeder> logger)
//     {
//         _context = context;
//         _logger = logger;
//     }

//     public async Task SeedAsync()
//     {
//         if (await _context.AccountHeads.AnyAsync())
//         {
//             _logger.LogInformation("Database already seeded.");
//             return;
//         }

//         _logger.LogInformation("Starting database seeding...");
//         int defaultTenantId = 1; // Default for seeding

//         // 1. Seed Groups
//         var groups = new List<AccountGroup>
//         {
//             new AccountGroup { Name = "Capital Account", PrimaryGroup = "Capital", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Current Assets", PrimaryGroup = "Current Assets", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Current Liabilities", PrimaryGroup = "Current Liabilities", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Sales Accounts", PrimaryGroup = "Sales Accounts", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Purchase Accounts", PrimaryGroup = "Purchase Accounts", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Direct Incomes", PrimaryGroup = "Direct Incomes", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Indirect Incomes", PrimaryGroup = "Indirect Incomes", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Direct Expenses", PrimaryGroup = "Direct Expenses", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Indirect Expenses", PrimaryGroup = "Indirect Expenses", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Assets", PrimaryGroup = "Fixed Assets", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Loans (Liability)", PrimaryGroup = "Secured Loans", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Cash-in-Hand", PrimaryGroup = "Cash-in-Hand", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Bank Accounts", PrimaryGroup = "Bank Accounts", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Duties & Taxes", PrimaryGroup = "Duties & Taxes", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Stock-in-Hand", PrimaryGroup = "Stock-in-Hand", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Sundry Debtors", PrimaryGroup = "Sundry Debtors", TenantId = defaultTenantId },
//             new AccountGroup { Name = "Sundry Creditors", PrimaryGroup = "Sundry Creditors", TenantId = defaultTenantId }
//         };
//         await _context.AccountGroups.AddRangeAsync(groups);
//         await _context.SaveChangesAsync();

//         // 2. Seed Default Ledgers
//         var cashGroup = groups.First(g => g.Name == "Cash-in-Hand");
//         var bankGroup = groups.First(g => g.Name == "Bank Accounts");
//         var salesGroup = groups.First(g => g.Name == "Sales Accounts");
//         var purchaseGroup = groups.First(g => g.Name == "Purchase Accounts");
//         var dutyGroup = groups.First(g => g.Name == "Duties & Taxes");
//         var capitalGroup = groups.First(g => g.Name == "Capital Account");

//         var ledgers = new List<AccountHead>
//         {
//             new AccountHead { Name = "Cash", GroupId = cashGroup.Id, OpeningBalance = 10000, TenantId = defaultTenantId },
//             new AccountHead { Name = "HDFC Bank", GroupId = bankGroup.Id, OpeningBalance = 50000, TenantId = defaultTenantId },
//             new AccountHead { Name = "Sales", GroupId = salesGroup.Id, TenantId = defaultTenantId },
//             new AccountHead { Name = "Purchases", GroupId = purchaseGroup.Id, TenantId = defaultTenantId },
//             new AccountHead { Name = "Output CGST", GroupId = dutyGroup.Id, TenantId = defaultTenantId },
//             new AccountHead { Name = "Output SGST", GroupId = dutyGroup.Id, TenantId = defaultTenantId },
//             new AccountHead { Name = "Input CGST", GroupId = dutyGroup.Id, TenantId = defaultTenantId },
//             new AccountHead { Name = "Input SGST", GroupId = dutyGroup.Id, TenantId = defaultTenantId },
//             new AccountHead { Name = "Capital Account", GroupId = capitalGroup.Id, OpeningBalance = 100000, TenantId = defaultTenantId }
//         };
//         await _context.AccountHeads.AddRangeAsync(ledgers);
//         await _context.SaveChangesAsync();

//         // 3. Seed Items & Godowns
//         var godown = new Godown { Name = "Main Warehouse", TenantId = defaultTenantId };
//         await _context.Godowns.AddAsync(godown);
//         await _context.SaveChangesAsync();

//         var items = new List<Item>
//         {
//             new Item { Name = "Laptop", HsnCode = "8471", Rate = 45000, GodownId = godown.Id, TenantId = defaultTenantId },
//             new Item { Name = "Mouse", HsnCode = "8471", Rate = 500, GodownId = godown.Id, TenantId = defaultTenantId },
//             new Item { Name = "Keyboard", HsnCode = "8471", Rate = 1200, GodownId = godown.Id, TenantId = defaultTenantId },
//             new Item { Name = "Monitor", HsnCode = "8528", Rate = 12000, GodownId = godown.Id, TenantId = defaultTenantId },
//             new Item { Name = "Printer", HsnCode = "8443", Rate = 15000, GodownId = godown.Id, TenantId = defaultTenantId }
//         };
//         await _context.Items.AddRangeAsync(items);
//         await _context.SaveChangesAsync();

//         _logger.LogInformation("Database seeding completed successfully.");
//     }
// }


using TaxAccount.Data;
using TaxAccount.Models.Accounting; // Ensure this matches your model namespace
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaxAccount.Services
{
    public class DbSeeder
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DbSeeder> _logger;

        public DbSeeder(AppDbContext context, ILogger<DbSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            if (await _context.AccountHeads.AnyAsync())
            {
                _logger.LogInformation("Database already seeded.");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            // 1. Seed Groups
            var groups = new List<AccountGroup>
            {
                new AccountGroup { Name = "Cash-in-Hand", PrimaryGroup = "Cash-in-Hand", TenantId = 1 },
                new AccountGroup { Name = "Bank Accounts", PrimaryGroup = "Bank Accounts", TenantId = 1 },
                new AccountGroup { Name = "Capital Account", PrimaryGroup = "Capital", TenantId = 1 },
                new AccountGroup { Name = "Sales Accounts", PrimaryGroup = "Sales Accounts", TenantId = 1 },
                new AccountGroup { Name = "Purchase Accounts", PrimaryGroup = "Purchase Accounts", TenantId = 1 },
                new AccountGroup { Name = "Duties & Taxes", PrimaryGroup = "Duties & Taxes", TenantId = 1 }
            };
            
            await _context.AccountGroups.AddRangeAsync(groups);
            await _context.SaveChangesAsync();

            // 2. Seed Default Ledgers (AccountHeads)
            // Since AccountHead doesn't have GroupId, we rely on Naming Conventions or ParentId hierarchy.
            // For simplicity in seeding, we create them as top-level nodes. 
            // In a real app, you might set ParentId to a root node representing the group.
            
            var ledgers = new List<AccountHead>
            {
                new AccountHead { Name = "Cash", Type = AccountType.Asset, OpeningBalance = 10000, TenantId = 1 },
                new AccountHead { Name = "HDFC Bank", Type = AccountType.Asset, OpeningBalance = 50000, TenantId = 1 },
                new AccountHead { Name = "Sales", Type = AccountType.Income, TenantId = 1 },
                new AccountHead { Name = "Purchases", Type = AccountType.Expense, TenantId = 1 },
                new AccountHead { Name = "Output CGST", Type = AccountType.Liability, TenantId = 1 },
                new AccountHead { Name = "Output SGST", Type = AccountType.Liability, TenantId = 1 },
                new AccountHead { Name = "Input CGST", Type = AccountType.Asset, TenantId = 1 },
                new AccountHead { Name = "Input SGST", Type = AccountType.Asset, TenantId = 1 },
                new AccountHead { Name = "Capital Account", Type = AccountType.Equity, OpeningBalance = 100000, TenantId = 1 }
            };

            await _context.AccountHeads.AddRangeAsync(ledgers);
            await _context.SaveChangesAsync();

            // 3. Seed Items & Godowns (Only if these entities exist in your DbContext)
            // Uncomment below only if you have DbSet<Item> and DbSet<Godown> in AppDbContext
            
            /*
            if (!_context.Godowns.Any())
            {
                var godown = new Godown { Name = "Main Warehouse", TenantId = 1 };
                await _context.Godowns.AddAsync(godown);
                await _context.SaveChangesAsync();

                var items = new List<Item>
                {
                    new Item { Name = "Laptop", HsnCode = "8471", Rate = 45000, GodownId = godown.Id, TenantId = 1 },
                    new Item { Name = "Mouse", HsnCode = "8471", Rate = 500, GodownId = godown.Id, TenantId = 1 }
                };
                await _context.Items.AddRangeAsync(items);
                await _context.SaveChangesAsync();
            }
            */

            _logger.LogInformation("Database seeding completed successfully.");
        }
    }
}