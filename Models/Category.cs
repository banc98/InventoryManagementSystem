namespace InvApp.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public ICollection<Item> Items { get; set; } = new List<Item>();
}