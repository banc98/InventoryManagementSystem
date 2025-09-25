namespace InvApp.Models;

public class Item
{
    public int Id { get; set; }
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public int ReorderLevel { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}
