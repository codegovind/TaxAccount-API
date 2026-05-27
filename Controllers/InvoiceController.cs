using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaxAccount.Authorization;
using TaxAccount.DTOs;
using TaxAccount.Services;

namespace TaxAccount.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        [HasPermission("invoices.view")]
        public async Task<IActionResult> GetAll()
        {
            var invoices = await _invoiceService.GetAllAsync();
            return Ok(invoices);
        }

        [HttpGet("{id}")]
        [HasPermission("invoices.view")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);
            return Ok(invoice);
        }

        [HttpPost]
        [HasPermission("invoices.create")]
        public async Task<IActionResult> Create(CreateInvoiceDto dto)
        {
            // Get logged in user id from JWT token
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var invoice = await _invoiceService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), 
                new { id = invoice.Id }, invoice);
        }

        [HttpDelete("{id}")]
        [HasPermission("invoices.approve")]
        public async Task<IActionResult> Delete(int id)
        {
            await _invoiceService.DeleteAsync(id);
            return NoContent();
        }
    }
}