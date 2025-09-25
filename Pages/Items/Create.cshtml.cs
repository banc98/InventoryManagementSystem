using InvApp.Data;
using InvApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InvApp.Pages.Items;

[Authorize(Roles = "Admin,Manager")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public ItemInput Input { get; set; } = new();
    public SelectList CategoryOptions { get; set; } = default!;

    public class ItemInput
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int ReorderLevel { get; set; } = 10;
        public bool IsActive { get; set; } = true;
    }

    public async Task OnGetAsync()
    {
        CategoryOptions = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        CategoryOptions = new SelectList(await _db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        if (!ModelState.IsValid) return Page();

        var item = new Item
        {
            Sku = Input.Sku.Trim(),
            Name = Input.Name.Trim(),
            CategoryId = Input.CategoryId,
            ReorderLevel = Input.ReorderLevel,
            IsActive = Input.IsActive
        };
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
