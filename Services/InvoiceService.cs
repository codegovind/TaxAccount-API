using Microsoft.EntityFrameworkCore;
using TaxAccount.Data;
using TaxAccount.DTOs;
using TaxAccount.Exceptions;
using TaxAccount.Helpers;
using TaxAccount.Models;
using TaxAccount.Services;

namespace TaxAccount.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(
            AppDbContext context,
            ITenantService tenantService,
            ILogger<InvoiceService> logger)
        {
            _context = context;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<List<InvoiceResponseDto>> GetAllAsync()
        {
            var invoices = await _context.Invoices
                .Where(i => i.InvoiceType == InvoiceType.Sale)
                .Include(i => i.Contact)
                .Include(i => i.CreatedBy)
                .Include(i => i.Items)
                    .ThenInclude(item => item.Product)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return invoices.Select(MapToResponseDto).ToList();
        }

        public async Task<InvoiceResponseDto> GetByIdAsync(int id)
        {
            var invoice = await _context.Invoices
                .Where(i => i.InvoiceType == InvoiceType.Sale)
                .Include(i => i.Contact)
                .Include(i => i.CreatedBy)
                .Include(i => i.Items)
                    .ThenInclude(item => item.Product)
                .Include(i => i.TransportDetail)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                throw new NotFoundException($"Invoice {id} not found");

            return MapToResponseDto(invoice);
        }

        public async Task<InvoiceResponseDto> CreateAsync(
            CreateInvoiceDto dto, int createdByUserId)
        {
            var tenantId = _tenantService.GetTenantId();

            // Get tenant for GST state logic
            var tenant = await _context.Tenants
                .FindAsync(tenantId);

            // Cash Sale Fallback — if no contact, use default cash customer
            int? contactId = dto.ContactId;
            Contact? contact = null;

            if (contactId.HasValue)
            {
                contact = await _context.Contacts
                    .FirstOrDefaultAsync(c => c.Id == contactId);

                if (contact == null)
                    throw new NotFoundException("Contact not found");
            }
            else
            {
                // Find default cash customer for this tenant
                contact = await _context.Contacts
                    .FirstOrDefaultAsync(c =>
                        c.TenantId == tenantId && c.IsDefault);

                contactId = contact?.Id;

                _logger.LogInformation(
                    "No contact provided - using default cash customer " +
                    "for tenant {TenantId}", tenantId);
            }

            // Generate invoice number
            var invoiceNumber = await GenerateInvoiceNumberAsync(
                tenantId, dto.InvoiceType);

            // Determine inter-state for GST
            bool isInterState = GstCalculator.IsInterState(
                tenant?.State, contact?.State);

            // Process items with SNAPSHOT of current prices
            var items = new List<InvoiceItem>();
            decimal subTotal = 0;
            decimal totalDiscount = 0;
            decimal totalTax = 0;

            await using var transaction = await _context.Database
                .BeginTransactionAsync();

            try
            {
                foreach (var itemDto in dto.Items)
                {
                    var product = await _context.Products
                        .FindAsync(itemDto.ProductId);

                    if (product == null)
                        throw new NotFoundException(
                            $"Product {itemDto.ProductId} not found");

                    // Stock validation for sales
                    if (dto.InvoiceType == InvoiceType.Sale &&
                        product.Stock < itemDto.Quantity)
                    {
                        throw new AppException(
                            $"Insufficient stock for {product.Name}. " +
                            $"Available: {product.Stock}", 400);
                    }

                    // SNAPSHOT — use price at time of invoice
                    var unitPrice = itemDto.UnitPrice > 0
                        ? itemDto.UnitPrice
                        : product.Price;

                    var itemSubTotal = itemDto.Quantity * unitPrice;
                    var discountAmt = itemSubTotal *
                        (itemDto.DiscountPercent / 100);
                    var taxableAmount = itemSubTotal - discountAmt;

                    // GST calculation - CGST/SGST or IGST
                    var (cgst, sgst, igst) = GstCalculator.CalculateGst(
                        taxableAmount,
                        itemDto.TaxPercent > 0
                            ? itemDto.TaxPercent
                            : product.GSTPercent,
                        isInterState);

                    var totalTaxAmt = cgst + sgst + igst;
                    var itemTotal = taxableAmount + totalTaxAmt;

                    items.Add(new InvoiceItem
                    {
                        TenantId = tenantId,
                        ProductId = itemDto.ProductId,
                        Description = itemDto.Description.Length > 0
                            ? itemDto.Description
                            : product.Name,
                        HsnCode = product.HsnCode, // Snapshot
                        Quantity = itemDto.Quantity,
                        Unit = product.Unit,        // Snapshot
                        UnitPrice = unitPrice,       // Snapshot
                        DiscountPercent = itemDto.DiscountPercent,
                        DiscountAmount = discountAmt,
                        TaxPercent = itemDto.TaxPercent > 0
                            ? itemDto.TaxPercent
                            : product.GSTPercent,   // Snapshot
                        TaxAmount = totalTaxAmt,
                        CgstPercent = isInterState ? 0
                            : (itemDto.TaxPercent > 0
                                ? itemDto.TaxPercent
                                : product.GSTPercent) / 2,
                        CgstAmount = cgst,
                        SgstPercent = isInterState ? 0
                            : (itemDto.TaxPercent > 0
                                ? itemDto.TaxPercent
                                : product.GSTPercent) / 2,
                        SgstAmount = sgst,
                        IgstPercent = isInterState
                            ? (itemDto.TaxPercent > 0
                                ? itemDto.TaxPercent
                                : product.GSTPercent)
                            : 0,
                        IgstAmount = igst,
                        TotalAmount = itemTotal
                    });

                    // Sale reduces stock
                    if (dto.InvoiceType == InvoiceType.Sale)
                        product.Stock -= itemDto.Quantity;

                    subTotal += itemSubTotal;
                    totalDiscount += discountAmt;
                    totalTax += totalTaxAmt;
                }

                var invoice = new Invoice
                {
                    TenantId = tenantId,
                    InvoiceNumber = invoiceNumber,
                    InvoiceType = dto.InvoiceType,
                    InvoiceDate = DateTime.UtcNow,
                    DueDate = dto.DueDate,
                    PaymentMethod = dto.PaymentMethod,
                    EntrySource = dto.EntrySource,
                    ContactId = contactId,
                    CreatedByUserId = createdByUserId,
                    Notes = dto.Notes,
                    SubTotal = subTotal,
                    DiscountAmount = totalDiscount,
                    TaxAmount = totalTax,
                    TotalAmount = subTotal - totalDiscount + totalTax,
                    Items = items
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Invoice {InvoiceNumber} created - Type: {InvoiceType}",
                    invoiceNumber, dto.InvoiceType);

                return await GetByIdAsync(invoice.Id);
            }
            catch (Exception ex) when (ex is not AppException
                and not NotFoundException)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Invoice creation failed");
                throw new AppException("Invoice creation failed", 500);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                throw new NotFoundException($"Invoice {id} not found");

            // Reverse stock changes for sales
            foreach (var item in invoice.Items)
            {
                var product = await _context.Products
                    .FindAsync(item.ProductId);

                if (product != null && invoice.InvoiceType == InvoiceType.Sale)
                {
                    product.Stock += item.Quantity;
                }
            }

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Invoice {Id} deleted", id);
            return true;
        }

        private async Task<string> GenerateInvoiceNumberAsync(
            int tenantId, InvoiceType type)
        {
            var year = DateTime.UtcNow.Year;
            var prefix = type == InvoiceType.Sale ? "INV" : "PUR";

            var count = await _context.Invoices
                .IgnoreQueryFilters()
                .CountAsync(i =>
                    i.TenantId == tenantId &&
                    i.InvoiceDate.Year == year &&
                    i.InvoiceType == type);

            return $"{prefix}-{year}-{(count + 1):D4}";
        }

        private static InvoiceResponseDto MapToResponseDto(Invoice i)
        {
            return new InvoiceResponseDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceType = i.InvoiceType.ToString(),
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                PaymentMethod = i.PaymentMethod.ToString(),
                EntrySource = i.EntrySource.ToString(),
                ContactId = i.ContactId,
                ContactName = i.Contact?.Name ?? "Cash Customer",
                CreatedByName = $"{i.CreatedBy.FirstName} {i.CreatedBy.LastName}",
                Notes = i.Notes,
                SubTotal = i.SubTotal,
                DiscountAmount = i.DiscountAmount,
                TaxAmount = i.TaxAmount,
                TotalAmount = i.TotalAmount,
                CreatedAt = i.CreatedAt,
                Items = i.Items.Select(item => new InvoiceItemResponseDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Description = item.Description,
                    HsnCode = item.HsnCode,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    UnitPrice = item.UnitPrice,
                    DiscountPercent = item.DiscountPercent,
                    DiscountAmount = item.DiscountAmount,
                    TaxPercent = item.TaxPercent,
                    TaxAmount = item.TaxAmount,
                    CgstPercent = item.CgstPercent,
                    CgstAmount = item.CgstAmount,
                    SgstPercent = item.SgstPercent,
                    SgstAmount = item.SgstAmount,
                    IgstPercent = item.IgstPercent,
                    IgstAmount = item.IgstAmount,
                    TotalAmount = item.TotalAmount
                }).ToList()
            };
        }
    }
}