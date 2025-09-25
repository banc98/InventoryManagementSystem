using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InvApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Connection string (make sure appsettings*.json has: "Data Source=app.db")
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString)); // <-- using SQLite right now

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity with Roles
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // flip to true if you add email confirm
})
.AddRoles<IdentityRole>() // <-- REQUIRED for RoleManager/roles seeding
.AddEntityFrameworkStores<ApplicationDbContext>();

// Razor + API Controllers
builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Optional: quick debug endpoint to verify DB wiring
app.MapGet("/debug/db", async (ApplicationDbContext db) =>
{
    var counts = new
    {
        Items = await db.Items.CountAsync(),
        Categories = await db.Categories.CountAsync(),
        Warehouses = await db.Warehouses.CountAsync(),
        Transactions = await db.InventoryTransactions.CountAsync()
    };
    return Results.Ok(counts);
});

// Seed roles/admin + sample data
await SeedData.InitAsync(app.Services);

app.Run();