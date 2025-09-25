using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InvApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Read provider; default to Sqlite locally
var provider = builder.Configuration.GetValue<string>("DbProvider") ?? "Sqlite";
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase) ? "Data Source=app.db"
                                                                              : throw new InvalidOperationException("DefaultConnection missing."));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        options.UseSqlServer(connStr);
    else if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
        options.UseNpgsql(connStr);   // <-- Postgres
    else
        options.UseSqlite(connStr);   // default local
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity + Roles
builder.Services.AddDefaultIdentity<IdentityUser>(o => o.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

// Pipeline...
if (app.Environment.IsDevelopment()) app.UseMigrationsEndPoint();
else { app.UseExceptionHandler("/Error"); app.UseHsts(); }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Auto-apply EF migrations on startup (works for Sqlite & SQL Server)
await SeedData.InitAsync(app.Services);

app.Run();
