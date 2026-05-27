using Microsoft.EntityFrameworkCore;
using TaxAccount.Data;
using TaxAccount.DTOs;
using TaxAccount.Exceptions;
using TaxAccount.Helpers;
using TaxAccount.Models;

namespace TaxAccount.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(
            AppDbContext context,
            ITenantService tenantService,
            ILogger<PurchaseService> logger)
        {
            _context = context;
            _tenantService = tenantService;
            _logger = logger;
        }

        // ── Purchase Bills ──

        public async Task<List<PurchaseBillResponseDto>> GetAllBillsAsync()
        {
            var bills = await _context.PurchaseBills
                .Include(pb => pb.Contact)
                .Include(pb => pb.CreatedBy)
                .Include(pb => pb.Items)
                    .ThenInclude(item => item.Product)
                .OrderByDescending(pb => pb.CreatedAt)
                .ToListAsync();

            return bills.Select(MapToBillResponseDto).ToList();
        }

        public async Task<PurchaseBillResponseDto> GetBillByIdAsync(int id)
        {
            var bill = await _context.PurchaseBills
                .Include(pb => pb.Contact)
                .Include(pb => pb.CreatedBy)
                .Include(pb => pb.Items)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(pb => pb.Id == id);

            if (bill == null)
                throw new NotFoundException($"Purchase bill {id} not found");

            return MapToBillResponseDto(bill);
        }

        public async Task<PurchaseBillResponseDto> CreateBillAsync(
            CreatePurchaseBillDto dto, int userId)
        {
            var tenantId = _tenantService.GetTenantId();

            var tenant = await _context.Tenants.FindAsync(tenantId);

            // Cash vendor fallback
            int? contactId = dto.ContactId;
            Contact? contact = null;

            if (contactId.HasValue)
            {
                contact = await _context.Contacts
                    .FirstOrDefaultAsync(c => c.Id == contactId);
                if (contact == null)
                    throw new NotFoundException("Vendor not found");
            }
            else
            {
                contact = await _context.Contacts
                    .FirstOrDefaultAsync(c =>
                        c.TenantId == tenantId && c.IsDefault);
                contactId = contact?.Id;
            }

            bool isInterState = GstCalculator.IsInterState(
                tenant?.State, contact?.State);

            var billNumber = await GenerateBillNumberAsync(tenantId);

            var items = new List<PurchaseBillItem>();
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

                    var unitPrice = itemDto.UnitPrice > 0
                        ? itemDto.UnitPrice
                        : product.PurchasePrice;

                    var itemSubTotal = itemDto.Quantity * unitPrice;
                    var discountAmt = itemSubTotal *
                        (itemDto.DiscountPercent / 100);
                    var taxableAmount = itemSubTotal - discountAmt;

                    var taxPercent = itemDto.TaxPercent > 0
                        ? itemDto.TaxPercent
                        : product.GSTPercent;

                    var (cgst, sgst, igst) = GstCalculator.CalculateGst(
                        taxableAmount, taxPercent, isInterState);

                    var totalTaxAmt = cgst + sgst + igst;

                    items.Add(new PurchaseBillItem
                    {
                        TenantId = tenantId,
                        ProductId = itemDto.ProductId,
                        Description = itemDto.Description.Length > 0
                            ? itemDto.Description : product.Name,
                        HsnCode = product.HsnCode,
                        Quantity = itemDto.Quantity,
                        Unit = product.Unit,
                        UnitPrice = unitPrice,
                        DiscountPercent = itemDto.DiscountPercent,
                        DiscountAmount = discountAmt,
                        TaxPercent = taxPercent,
                        TaxAmount = totalTaxAmt,
                        CgstPercent = isInterState ? 0 : taxPercent / 2,
                        CgstAmount = cgst,
                        SgstPercent = isInterState ? 0 : taxPercent / 2,
                        SgstAmount = sgst,
                        IgstPercent = isInterState ? taxPercent : 0,
                        IgstAmount = igst,
                        TotalAmount = taxableAmount + totalTaxAmt
                    });

                    // Purchase increases stock
                    product.Stock += itemDto.Quantity;

                    // Update purchase price snapshot
                    if (unitPrice > 0)
                        product.PurchasePrice = unitPrice;

                    subTotal += itemSubTotal;
                    totalDiscount += discountAmt;
                    totalTax += totalTaxAmt;
                }

                var bill = new PurchaseBill
                {
                    TenantId = tenantId,
                    BillNumber = billNumber,
                    BillDate = dto.BillDate,
                    DueDate = dto.DueDate ?? dto.BillDate.AddDays(30),
                    PaymentMethod = dto.PaymentMethod,
                    EntrySource = EntrySource.FullAccounting,
                    VendorBillNumber = dto.VendorBillNumber,
                    ContactId = contactId,
                    CreatedByUserId = userId,
                    Notes = dto.Notes,
                    SubTotal = subTotal,
                    DiscountAmount = totalDiscount,
                    TaxAmount = totalTax,
                    TotalAmount = subTotal - totalDiscount + totalTax,
                    Items = items
                };

                _context.PurchaseBills.Add(bill);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Purchase bill {BillNumber} created", billNumber);

                return await GetBillByIdAsync(bill.Id);
            }
            catch (Exception ex) when (
                ex is not AppException and not NotFoundException)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Purchase bill creation failed");
                throw new AppException("Purchase bill creation failed", 500);
            }
        }

        public async Task<bool> DeleteBillAsync(int id)
        {
            var bill = await _context.PurchaseBills
                .Include(pb => pb.Items)
                .FirstOrDefaultAsync(pb => pb.Id == id);

            if (bill == null)
                throw new NotFoundException($"Purchase bill {id} not found");

            // Reverse stock
            foreach (var item in bill.Items)
            {
                var product = await _context.Products
                    .FindAsync(item.ProductId);
                if (product != null)
                    product.Stock -= item.Quantity;
            }

            _context.PurchaseBills.Remove(bill);
            await _context.SaveChangesAsync();
            return true;
        }

        // ── Purchase Orders ──

        public async Task<List<PurchaseOrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _context.PurchaseOrders
                .Include(po => po.Contact)
                .Include(po => po.CreatedBy)
                .Include(po => po.Items)
                    .ThenInclude(item => item.Product)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();

            return orders.Select(MapToOrderResponseDto).ToList();
        }

        public async Task<PurchaseOrderResponseDto> GetOrderByIdAsync(int id)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Contact)
                .Include(po => po.CreatedBy)
                .Include(po => po.Items)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null)
                throw new NotFoundException(
                    $"Purchase order {id} not found");

            return MapToOrderResponseDto(order);
        }

        public async Task<PurchaseOrderResponseDto> CreateOrderAsync(
            CreatePurchaseOrderDto dto, int userId)
        {
            var tenantId = _tenantService.GetTenantId();
            var tenant = await _context.Tenants.FindAsync(tenantId);

            int? contactId = dto.ContactId;
            Contact? contact = null;

            if (contactId.HasValue)
            {
                contact = await _context.Contacts
                    .FirstOrDefaultAsync(c => c.Id == contactId);
            }

            bool isInterState = GstCalculator.IsInterState(
                tenant?.State, contact?.State);

            var orderNumber = await GenerateOrderNumberAsync(tenantId);

            var items = new List<PurchaseOrderItem>();
            decimal subTotal = 0;
            decimal totalDiscount = 0;
            decimal totalTax = 0;

            foreach (var itemDto in dto.Items)
            {
                var product = await _context.Products
                    .FindAsync(itemDto.ProductId);

                if (product == null)
                    throw new NotFoundException(
                        $"Product {itemDto.ProductId} not found");

                var unitPrice = itemDto.UnitPrice > 0
                    ? itemDto.UnitPrice
                    : product.PurchasePrice;

                var itemSubTotal = itemDto.Quantity * unitPrice;
                var discountAmt = itemSubTotal *
                    (itemDto.DiscountPercent / 100);
                var taxableAmount = itemSubTotal - discountAmt;

                var taxPercent = itemDto.TaxPercent > 0
                    ? itemDto.TaxPercent
                    : product.GSTPercent;

                var (cgst, sgst, igst) = GstCalculator.CalculateGst(
                    taxableAmount, taxPercent, isInterState);

                var totalTaxAmt = cgst + sgst + igst;

                items.Add(new PurchaseOrderItem
                {
                    TenantId = tenantId,
                    ProductId = itemDto.ProductId,
                    Description = itemDto.Description.Length > 0
                        ? itemDto.Description : product.Name,
                    HsnCode = product.HsnCode,
                    Quantity = itemDto.Quantity,
                    Unit = product.Unit,
                    UnitPrice = unitPrice,
                    DiscountPercent = itemDto.DiscountPercent,
                    DiscountAmount = discountAmt,
                    TaxPercent = taxPercent,
                    TaxAmount = totalTaxAmt,
                    CgstPercent = isInterState ? 0 : taxPercent / 2,
                    CgstAmount = cgst,
                    SgstPercent = isInterState ? 0 : taxPercent / 2,
                    SgstAmount = sgst,
                    IgstPercent = isInterState ? taxPercent : 0,
                    IgstAmount = igst,
                    TotalAmount = taxableAmount + totalTaxAmt
                });

                subTotal += itemSubTotal;
                totalDiscount += discountAmt;
                totalTax += totalTaxAmt;
            }

            var order = new PurchaseOrder
            {
                TenantId = tenantId,
                OrderNumber = orderNumber,
                OrderDate = dto.OrderDate,
                ExpectedDate = dto.ExpectedDate,
                Status = PurchaseOrderStatus.Draft,
                ContactId = contactId,
                CreatedByUserId = userId,
                Notes = dto.Notes,
                SubTotal = subTotal,
                DiscountAmount = totalDiscount,
                TaxAmount = totalTax,
                TotalAmount = subTotal - totalDiscount + totalTax,
                Items = items
            };

            _context.PurchaseOrders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Purchase order {OrderNumber} created", orderNumber);

            return await GetOrderByIdAsync(order.Id);
        }

        public async Task<PurchaseOrderResponseDto> UpdateOrderStatusAsync(
            int id, UpdatePurchaseOrderStatusDto dto)
        {
            var order = await _context.PurchaseOrders.FindAsync(id);
            if (order == null)
                throw new NotFoundException(
                    $"Purchase order {id} not found");

            if (order.Status == PurchaseOrderStatus.Cancelled)
                throw new AppException("Cannot update cancelled order");

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetOrderByIdAsync(id);
        }

        public async Task<PurchaseBillResponseDto> ConvertOrderToBillAsync(
            int orderId, int userId)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == orderId);

            if (order == null)
                throw new NotFoundException("Purchase order not found");

            if (order.Status == PurchaseOrderStatus.Cancelled)
                throw new AppException("Cannot convert cancelled order");

            // Convert order items to bill dto
            var billDto = new CreatePurchaseBillDto
            {
                ContactId = order.ContactId,
                BillDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                Notes = $"Converted from PO: {order.OrderNumber}",
                Items = order.Items.Select(i => new CreatePurchaseItemDto
                {
                    ProductId = i.ProductId,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountPercent = i.DiscountPercent,
                    TaxPercent = i.TaxPercent
                }).ToList()
            };

            // Mark order as received
            order.Status = PurchaseOrderStatus.Received;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await CreateBillAsync(billDto, userId);
        }

        // ── Number Generators ──

        private async Task<string> GenerateBillNumberAsync(int tenantId)
        {
            var year = DateTime.UtcNow.Year;
            var count = await _context.PurchaseBills
                .IgnoreQueryFilters()
                .CountAsync(pb =>
                    pb.TenantId == tenantId &&
                    pb.BillDate.Year == year);

            return $"PUR-{year}-{(count + 1):D4}";
        }

        private async Task<string> GenerateOrderNumberAsync(int tenantId)
        {
            var year = DateTime.UtcNow.Year;
            var count = await _context.PurchaseOrders
                .IgnoreQueryFilters()
                .CountAsync(po =>
                    po.TenantId == tenantId &&
                    po.OrderDate.Year == year);

            return $"PO-{year}-{(count + 1):D4}";
        }

        // ── Mappers ──

        private static PurchaseBillResponseDto MapToBillResponseDto(
            PurchaseBill pb) => new()
        {
            Id = pb.Id,
            BillNumber = pb.BillNumber,
            VendorBillNumber = pb.VendorBillNumber,
            BillDate = pb.BillDate,
            DueDate = pb.DueDate,
            PaymentMethod = pb.PaymentMethod.ToString(),
            ContactId = pb.ContactId,
            VendorName = pb.Contact?.Name ?? "Cash Vendor",
            CreatedByName =
                $"{pb.CreatedBy.FirstName} {pb.CreatedBy.LastName}",
            Notes = pb.Notes,
            SubTotal = pb.SubTotal,
            DiscountAmount = pb.DiscountAmount,
            TaxAmount = pb.TaxAmount,
            TotalAmount = pb.TotalAmount,
            CreatedAt = pb.CreatedAt,
            Items = pb.Items.Select(item => new PurchaseItemResponseDto
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

        private static PurchaseOrderResponseDto MapToOrderResponseDto(
            PurchaseOrder po) => new()
        {
            Id = po.Id,
            OrderNumber = po.OrderNumber,
            OrderDate = po.OrderDate,
            ExpectedDate = po.ExpectedDate,
            Status = po.Status.ToString(),
            ContactId = po.ContactId,
            VendorName = po.Contact?.Name ?? "No Vendor",
            CreatedByName =
                $"{po.CreatedBy.FirstName} {po.CreatedBy.LastName}",
            Notes = po.Notes,
            SubTotal = po.SubTotal,
            DiscountAmount = po.DiscountAmount,
            TaxAmount = po.TaxAmount,
            TotalAmount = po.TotalAmount,
            CreatedAt = po.CreatedAt,
            Items = po.Items.Select(item => new PurchaseItemResponseDto
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