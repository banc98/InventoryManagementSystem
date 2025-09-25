namespace InvApp.Models;

public class InventoryTransaction
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public Item Item { get; set; } = default!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = default!;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public int QuantityDelta { get; set; } // + receive, - issue
    public string? Note { get; set; }
}