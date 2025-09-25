using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InvApp.Models;

namespace InvApp.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Item>().HasIndex(x => x.Sku).IsUnique();
        b.Entity<Item>().Property(x => x.Name).HasMaxLength(200).IsRequired();

        b.Entity<Category>().HasIndex(x => x.Name).IsUnique();

        b.Entity<Warehouse>().HasIndex(x => x.Code).IsUnique();
    }
}