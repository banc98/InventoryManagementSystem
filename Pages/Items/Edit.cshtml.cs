using InvApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InvApp.Pages.Items;

[Authorize(Roles = "Admin,Manager")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public int Id { get; set; }
    [BindProperty] public string Sku { get; set; } = string.Empty;
    [BindProperty] public string Name { get; set; } = string.Empty;
    [BindProperty] public int CategoryId { get; set; }
    [BindProperty] public int ReorderLevel { get; set; }
    [BindProperty] public bool IsActive { get; set; }

    public SelectList CategoryOptions { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await _db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        if (item is null) return NotFound();

        Id = item.Id;
        Sku = item.Sku;
        Name = item.Name;
        CategoryId = item.CategoryId;
        ReorderLevel = item.ReorderLevel;
        IsActive = item.IsActive;

        CategoryOptions = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", CategoryId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            CategoryOptions = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", CategoryId);
            return Page();
        }

        var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == Id);
        if (item is null) return NotFound();

        item.Sku = Sku.Trim();
        item.Name = Name.Trim();
        item.CategoryId = CategoryId;
        item.ReorderLevel = ReorderLevel;
        item.IsActive = IsActive;

        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
