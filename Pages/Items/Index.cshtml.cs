using InvApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InvApp.Pages.Items;

[Authorize] // any logged-in user can view
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public record Row(int Id, string Sku, string Name, string Category, int OnHand, int ReorderLevel, bool IsActive);
    public List<Row> Rows { get; set; } = new();

    public async Task OnGetAsync()
    {
        Rows = await _db.Items
            .Include(i => i.Category)
            .Select(i => new Row(
                i.Id,
                i.Sku,
                i.Name,
                i.Category.Name,
                _db.InventoryTransactions.Where(t => t.ItemId == i.Id).Sum(t => (int?)t.QuantityDelta) ?? 0,
                i.ReorderLevel,
                i.IsActive
            ))
            .AsNoTracking()
            .ToListAsync();
    }
}
