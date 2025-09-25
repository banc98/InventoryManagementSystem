using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvApp.Data;
using InvApp.Models;

namespace InvApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // must be logged in for any endpoint
public class ItemsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ItemsController(ApplicationDbContext db) => _db = db;

    // GET: /api/items
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Items
            .Include(i => i.Category)
            .Select(i => new {
                i.Id, i.Sku, i.Name,
                Category = i.Category.Name,
                i.ReorderLevel,
                i.IsActive,
                OnHand = _db.InventoryTransactions
                             .Where(t => t.ItemId == i.Id)
                             .Sum(t => (int?)t.QuantityDelta) ?? 0
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(items);
    }

    // GET: /api/items/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _db.Items
            .Include(i => i.Category)
            .Where(i => i.Id == id)
            .Select(i => new {
                i.Id, i.Sku, i.Name,
                CategoryId = i.CategoryId,
                Category = i.Category.Name,
                i.ReorderLevel,
                i.IsActive,
                OnHand = _db.InventoryTransactions
                             .Where(t => t.ItemId == i.Id)
                             .Sum(t => (int?)t.QuantityDelta) ?? 0
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    public record ItemCreateDto(string Sku, string Name, int CategoryId, int ReorderLevel = 10, bool IsActive = true);
    public record ItemUpdateDto(string Sku, string Name, int CategoryId, int ReorderLevel, bool IsActive);

    // POST: /api/items
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(ItemCreateDto dto)
    {
        var item = new Item {
            Sku = dto.Sku.Trim(),
            Name = dto.Name.Trim(),
            CategoryId = dto.CategoryId,
            ReorderLevel = dto.ReorderLevel,
            IsActive = dto.IsActive
        };
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = item.Id }, new { item.Id });
    }

    // PUT: /api/items/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, ItemUpdateDto dto)
    {
        var item = await _db.Items.FindAsync(id);
        if (item is null) return NotFound();

        item.Sku = dto.Sku.Trim();
        item.Name = dto.Name.Trim();
        item.CategoryId = dto.CategoryId;
        item.ReorderLevel = dto.ReorderLevel;
        item.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: /api/items/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item is null) return NotFound();
        _db.Items.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}