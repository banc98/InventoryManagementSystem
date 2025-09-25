using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InvApp.Areas.Admin.Pages.Users;

public class CreateModel : PageModel
{
    private readonly UserManager<IdentityUser> _userMgr;
    private readonly RoleManager<IdentityRole> _roleMgr;

    public CreateModel(UserManager<IdentityUser> userMgr, RoleManager<IdentityRole> roleMgr)
    {
        _userMgr = userMgr;
        _roleMgr = roleMgr;
    }

    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public List<string> SelectedRoles { get; set; } = new();

    public List<string> AllRoles { get; set; } = new();

    public void OnGet()
    {
        AllRoles = _roleMgr.Roles.Select(r => r.Name!).ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AllRoles = _roleMgr.Roles.Select(r => r.Name!).ToList();

        var email = Email?.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(Password))
        {
            ModelState.AddModelError("", "Email and Password are required.");
            return Page();
        }

        var user = new IdentityUser
        {
            Email = email,
            UserName = email,          // ✅ username always = email
            EmailConfirmed = true
        };

        var create = await _userMgr.CreateAsync(user, Password);
        if (!create.Succeeded)
        {
            foreach (var e in create.Errors) ModelState.AddModelError("", e.Description);
            return Page();
        }

        if (SelectedRoles.Any())
        {
            var addRoles = await _userMgr.AddToRolesAsync(user, SelectedRoles);
            if (!addRoles.Succeeded)
            {
                foreach (var e in addRoles.Errors) ModelState.AddModelError("", e.Description);
                return Page();
            }
        }

        return RedirectToPage("Index");
    }
}
