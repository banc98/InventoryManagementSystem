using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InvApp.Models;

namespace InvApp.Data;

public static class SeedData
{
    // TODO: set these to the admin you want to keep
    private const string AdminEmail = "admin@brooke";
    private const string AdminPassword = "Brooke98!";

    // If you previously used a different seeded admin and want to remove/demote it, set here (or leave null)
    private const string? LegacyAdminEmail = "admin@inv.local"; // or null to skip cleanup

    public static async Task InitAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db       = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userMgr  = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleMgr  = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply any pending EF Core migrations
        await db.Database.MigrateAsync();

        // --- Ensure roles exist ---
        string[] roles = new[] { "Admin", "Manager", "Clerk", "Viewer" };
        foreach (var r in roles)
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new IdentityRole(r));

        // --- Ensure the primary Admin exists (username = email) ---
        var admin = await userMgr.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new IdentityUser
            {
                Email = AdminEmail,
                UserName = AdminEmail,
                EmailConfirmed = true
            };
            var created = await userMgr.CreateAsync(admin, AdminPassword);
            if (!created.Succeeded)
            {
                // Log/throw in dev to surface why the seed failed
                var msg = string.Join("; ", created.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new InvalidOperationException($"Failed to create admin user: {msg}");
            }
        }
        // make sure admin is in Admin role
        if (!await userMgr.IsInRoleAsync(admin, "Admin"))
            await userMgr.AddToRoleAsync(admin, "Admin");

        // --- One-time sync: enforce username = email for ALL users ---
        var allUsers = await userMgr.Users.ToListAsync();
        foreach (var u in allUsers)
        {
            if (!string.IsNullOrWhiteSpace(u.Email) && u.UserName != u.Email)
            {
                u.UserName = u.Email;
                var upd = await userMgr.UpdateAsync(u);
                // If update fails, continue; you can log this if desired
            }
        }

        // --- (Optional) Legacy admin cleanup ---
        if (!string.IsNullOrWhiteSpace(LegacyAdminEmail) &&
            !LegacyAdminEmail!.Equals(AdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            var legacy = await userMgr.FindByEmailAsync(LegacyAdminEmail);
            if (legacy is not null)
            {
                // If legacy is an Admin and would become the last Admin after deletion,
                // we remove its Admin role first to ensure at least one Admin remains (the new one).
                if (await userMgr.IsInRoleAsync(legacy, "Admin"))
                {
                    await userMgr.RemoveFromRoleAsync(legacy, "Admin");
                }

                // You can delete the legacy user entirely, or keep them demoted:
                // await userMgr.DeleteAsync(legacy);
            }
        }

        // --- (Optional) Sample domain seed so the app has data on first run ---
        if (!await db.Categories.AnyAsync())
        {
            var cat = new Category { Name = "Widgets" };
            var wh  = new Warehouse { Code = "MAIN", Name = "Main Warehouse" };
            var item = new Item { Sku = "WID-001", Name = "Widget A", Category = cat, ReorderLevel = 5, IsActive = true };

            db.Categories.Add(cat);
            db.Warehouses.Add(wh);
            db.Items.Add(item);
            await db.SaveChangesAsync();

            db.InventoryTransactions.AddRange(
                new InventoryTransaction { ItemId = item.Id, WarehouseId = wh.Id, QuantityDelta = +10, Note = "Initial stock" },
                new InventoryTransaction { ItemId = item.Id, WarehouseId = wh.Id, QuantityDelta = -3,  Note = "Test issue" }
            );
            await db.SaveChangesAsync();
        }
    }
}
