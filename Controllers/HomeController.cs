using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaxAccount.Authorization;
using TaxAccount.Data;
using TaxAccount.Models;

namespace TaxAccount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("dashboard")]
        [HasPermission("reports.view")]
        public async Task<IActionResult> GetDashboard()
        {
            var totalInvoices = await _context.Invoices
            .Where(i => i.InvoiceType == InvoiceType.Sale)
            .CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalUsers = await _context.Users.CountAsync();

            var totalRevenue = await _context.Invoices
                .Where(i => i.InvoiceType == InvoiceType.Sale)
                .SumAsync(i => i.TotalAmount);

            var recentInvoices = await _context.Invoices
                .Where(i => i.InvoiceType == InvoiceType.Sale)
                .Include(i => i.Contact)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    CustomerName = i.Contact != null ? i.Contact.Name : "Cash Sale",
                    i.TotalAmount,
                    i.InvoiceDate
                })
                .ToListAsync();

            return Ok(new
            {
                totalInvoices,
                totalProducts,
                totalUsers,
                totalRevenue,
                recentInvoices
            });
        }
    }
}