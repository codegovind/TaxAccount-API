using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaxAccount.Data;
using TaxAccount.Models.Inventory;
using TaxAccount.Services;

namespace TaxAccount.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StockJournalController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly ILogger<StockJournalController> _logger;

    public StockJournalController(
        AppDbContext context,
        ITenantService tenantService,
        ILogger<StockJournalController> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _logger = logger;
    }

    /// <summary>
    /// Get all stock journals with optional filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStockJournals(
        [FromQuery] StockJournalType? type,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? godownId)
    {
        var tenantId = _tenantService.GetTenantId();
        var query = _context.StockJournals
            .Include(sj => sj.SourceGodown)
            .Include(sj => sj.DestinationGodown)
            .Include(sj => sj.CreatedBy)
            .Where(sj => sj.TenantId == tenantId)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(sj => sj.JournalType == type);

        if (fromDate.HasValue)
            query = query.Where(sj => sj.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(sj => sj.Date <= toDate.Value);

        if (godownId.HasValue)
            query = query.Where(sj => 
                sj.SourceGodownId == godownId || 
                sj.DestinationGodownId == godownId);

        var journals = await query.OrderByDescending(sj => sj.Date).ToListAsync();
        return Ok(journals);
    }

    /// <summary>
    /// Get stock journal by ID with all items
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStockJournal(int id)
    {
        var tenantId = _tenantService.GetTenantId();
        var journal = await _context.StockJournals
            .Include(sj => sj.SourceGodown)
            .Include(sj => sj.DestinationGodown)
            .Include(sj => sj.CreatedBy)
            .Include(sj => sj.ConsumedItems).ThenInclude(ci => ci.Item)
            .Include(sj => sj.ProducedItems).ThenInclude(pi => pi.Item)
            .Include(sj => sj.TransferItems).ThenInclude(ti => ti.Item)
            .FirstOrDefaultAsync(sj => sj.Id == id && sj.TenantId == tenantId);

        if (journal == null)
            return NotFound(new { message = "Stock journal not found" });

        return Ok(journal);
    }

    /// <summary>
    /// Create new stock journal (manufacturing, transfer, etc.)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateStockJournal([FromBody] CreateStockJournalDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tenantId = _tenantService.GetTenantId();
        var userId = _tenantService.GetUserId();

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Generate voucher number
            var voucherNumber = await GenerateVoucherNumberAsync(tenantId, dto.JournalType);

            var journal = new StockJournal
            {
                TenantId = tenantId,
                VoucherNumber = voucherNumber,
                JournalType = dto.JournalType,
                Date = dto.Date,
                SourceGodownId = dto.SourceGodownId,
                DestinationGodownId = dto.DestinationGodownId,
                Narration = dto.Narration,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add items based on journal type
            switch (dto.JournalType)
            {
                case StockJournalType.Manufacturing:
                    foreach (var item in dto.ConsumedItems)
                    {
                        journal.ConsumedItems.Add(new StockJournalItem
                        {
                            ItemId = item.ItemId,
                            Quantity = item.Quantity,
                            Unit = item.Unit,
                            Rate = item.Rate,
                            Amount = item.Quantity * (item.Rate ?? 0),
                            IsConsumed = true
                        });
                    }
                    foreach (var item in dto.ProducedItems)
                    {
                        journal.ProducedItems.Add(new StockJournalItem
                        {
                            ItemId = item.ItemId,
                            Quantity = item.Quantity,
                            Unit = item.Unit,
                            Rate = item.Rate,
                            Amount = item.Quantity * (item.Rate ?? 0),
                            IsConsumed = false
                        });
                    }
                    break;

                case StockJournalType.GodownTransfer:
                    foreach (var item in dto.TransferItems)
                    {
                        journal.TransferItems.Add(new StockJournalItem
                        {
                            ItemId = item.ItemId,
                            Quantity = item.Quantity,
                            Unit = item.Unit,
                            Rate = item.Rate,
                            Amount = item.Quantity * (item.Rate ?? 0),
                            StockBatchId = item.StockBatchId
                        });
                    }
                    break;

                default:
                    foreach (var item in dto.TransferItems)
                    {
                        journal.TransferItems.Add(new StockJournalItem
                        {
                            ItemId = item.ItemId,
                            Quantity = item.Quantity,
                            Unit = item.Unit,
                            Rate = item.Rate,
                            Amount = item.Quantity * (item.Rate ?? 0),
                            IsConsumed = dto.JournalType == StockJournalType.MaterialIssue
                        });
                    }
                    break;
            }

            _context.StockJournals.Add(journal);
            await _context.SaveChangesAsync();

            // Update stock levels
            await UpdateStockLevelsAsync(journal);

            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetStockJournal), new { id = journal.Id }, journal);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating stock journal");
            return StatusCode(500, new { message = "Error creating stock journal", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete stock journal (reverses stock changes)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStockJournal(int id)
    {
        var tenantId = _tenantService.GetTenantId();
        var journal = await _context.StockJournals
            .Include(sj => sj.ConsumedItems)
            .Include(sj => sj.ProducedItems)
            .Include(sj => sj.TransferItems)
            .FirstOrDefaultAsync(sj => sj.Id == id && sj.TenantId == tenantId);

        if (journal == null)
            return NotFound(new { message = "Stock journal not found" });

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Reverse stock changes before deleting
            await ReverseStockChangesAsync(journal);

            _context.StockJournals.Remove(journal);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new { message = "Stock journal deleted successfully" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting stock journal");
            return StatusCode(500, new { message = "Error deleting stock journal", error = ex.Message });
        }
    }

    #region Helper Methods

    private async Task<string> GenerateVoucherNumberAsync(int tenantId, StockJournalType type)
    {
        var prefix = type switch
        {
            StockJournalType.Manufacturing => "MFG",
            StockJournalType.GodownTransfer => "TRF",
            StockJournalType.MaterialIssue => "ISS",
            StockJournalType.MaterialReceipt => "RCT",
            StockJournalType.ScrapAdjustment => "SCP",
            _ => "SJ"
        };

        var year = DateTime.UtcNow.ToString("yy");
        var lastVoucher = await _context.StockJournals
            .Where(sj => sj.TenantId == tenantId && 
                        sj.VoucherNumber.StartsWith($"{prefix}/{year}/"))
            .OrderByDescending(sj => sj.VoucherNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastVoucher != null && 
            int.TryParse(lastVoucher.VoucherNumber.Split('/').LastOrDefault(), out var lastNum))
        {
            nextNumber = lastNum + 1;
        }

        return $"{prefix}/{year}/{nextNumber:D4}";
    }

    private async Task UpdateStockLevelsAsync(StockJournal journal)
    {
        // Decrease consumed/issued items
        foreach (var item in journal.ConsumedItems.Union(journal.TransferItems.Where(t => t.IsConsumed)))
        {
            var stockBatch = await _context.StockBatches
                .FirstOrDefaultAsync(sb => sb.ItemId == item.ItemId && 
                                          sb.GodownId == journal.SourceGodownId);
            
            if (stockBatch != null)
            {
                stockBatch.Quantity -= item.Quantity;
            }
        }

        // Increase produced/received items
        foreach (var item in journal.ProducedItems.Union(journal.TransferItems.Where(t => !t.IsConsumed)))
        {
            var existingBatch = await _context.StockBatches
                .FirstOrDefaultAsync(sb => sb.ItemId == item.ItemId && 
                                          sb.GodownId == journal.DestinationGodownId);
            
            if (existingBatch != null)
            {
                existingBatch.Quantity += item.Quantity;
            }
            else
            {
                // Create new batch
                _context.StockBatches.Add(new StockBatch
                {
                    TenantId = journal.TenantId,
                    ItemId = item.ItemId,
                    GodownId = journal.DestinationGodownId ?? 1,
                    BatchNumber = $"BATCH-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    Quantity = item.Quantity,
                    ManufactureDate = journal.Date,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task ReverseStockChangesAsync(StockJournal journal)
    {
        // Reverse the stock changes (opposite of UpdateStockLevelsAsync)
        foreach (var item in journal.ConsumedItems.Union(journal.TransferItems.Where(t => t.IsConsumed)))
        {
            var stockBatch = await _context.StockBatches
                .FirstOrDefaultAsync(sb => sb.ItemId == item.ItemId);
            
            if (stockBatch != null)
            {
                stockBatch.Quantity += item.Quantity;
            }
        }

        foreach (var item in journal.ProducedItems.Union(journal.TransferItems.Where(t => !t.IsConsumed)))
        {
            var stockBatch = await _context.StockBatches
                .FirstOrDefaultAsync(sb => sb.ItemId == item.ItemId);
            
            if (stockBatch != null)
            {
                stockBatch.Quantity -= item.Quantity;
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion
}

// DTOs for Stock Journal
public class CreateStockJournalDto
{
    public StockJournalType JournalType { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int? SourceGodownId { get; set; }
    public int? DestinationGodownId { get; set; }
    public string Narration { get; set; } = string.Empty;
    
    public List<StockJournalItemDto> ConsumedItems { get; set; } = new();
    public List<StockJournalItemDto> ProducedItems { get; set; } = new();
    public List<StockJournalItemDto> TransferItems { get; set; } = new();
}

public class StockJournalItemDto
{
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? Rate { get; set; }
    public int? StockBatchId { get; set; }
}
