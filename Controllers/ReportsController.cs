using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvApp.Data;

namespace InvApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ReportsController(ApplicationDbContext db) => _db = db;

    // GET: /api/reports/low-stock
    [HttpGet("low-stock")]
    public async Task<IActionResult> LowStock()
    {
        var rows = await _db.Items
            .Select(i => new {
                i.Id, i.Sku, i.Name,
                OnHand = _db.InventoryTransactions
                            .Where(t => t.ItemId == i.Id)
                            .Sum(t => (int?)t.QuantityDelta) ?? 0,
                i.ReorderLevel
            })
            .Where(x => x.OnHand <= x.ReorderLevel)
            .AsNoTracking()
            .ToListAsync();

        return Ok(rows);
    }

    // GET: /api/reports/history/5?from=2024-01-01&to=2025-12-31
    [HttpGet("history/{itemId:int}")]
    public async Task<IActionResult> History(int itemId, DateTime? from = null, DateTime? to = null)
    {
        from ??= DateTime.UtcNow.AddDays(-30);
        to   ??= DateTime.UtcNow;

        var data = await _db.InventoryTransactions
            .Where(t => t.ItemId == itemId && t.OccurredAt >= from && t.OccurredAt <= to)
            .OrderBy(t => t.OccurredAt)
            .Select(t => new {
                t.OccurredAt, t.QuantityDelta, t.Note, t.WarehouseId
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(data);
    }
}