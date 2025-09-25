using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InvApp.Areas.Admin.Pages.Users;

public class EditModel : PageModel
{
    private readonly UserManager<IdentityUser> _userMgr;
    private readonly RoleManager<IdentityRole> _roleMgr;

    public EditModel(UserManager<IdentityUser> userMgr, RoleManager<IdentityRole> roleMgr)
    {
        _userMgr = userMgr;
        _roleMgr = roleMgr;
    }

    [BindProperty] public string Id { get; set; } = string.Empty;
    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string NewPassword { get; set; } = string.Empty;
    [BindProperty] public List<string> SelectedRoles { get; set; } = new();

    public List<string> AllRoles { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var user = await _userMgr.FindByIdAsync(id);
        if (user == null) return NotFound();

        Id = user.Id;
        Email = user.Email ?? "";
        SelectedRoles = (await _userMgr.GetRolesAsync(user)).ToList();
        AllRoles = _roleMgr.Roles.Select(r => r.Name!).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AllRoles = _roleMgr.Roles.Select(r => r.Name!).ToList();

        var user = await _userMgr.FindByIdAsync(Id);
        if (user == null) return NotFound();

        user.Email = Email.Trim();
        user.UserName = user.Email;       // ✅ keep username in sync with email

        var upd = await _userMgr.UpdateAsync(user);
        if (!upd.Succeeded)
        {
            foreach (var e in upd.Errors) ModelState.AddModelError("", e.Description);
            return Page();
        }

        // Reset password if provided
        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            var token = await _userMgr.GeneratePasswordResetTokenAsync(user);
            var pass = await _userMgr.ResetPasswordAsync(user, token, NewPassword);
            if (!pass.Succeeded)
            {
                foreach (var e in pass.Errors) ModelState.AddModelError("", e.Description);
                return Page();
            }
        }

        // Update roles
        var currentRoles = await _userMgr.GetRolesAsync(user);
        var remove = currentRoles.Except(SelectedRoles);
        var add = SelectedRoles.Except(currentRoles);

        if (remove.Any()) await _userMgr.RemoveFromRolesAsync(user, remove);
        if (add.Any()) await _userMgr.AddToRolesAsync(user, add);

        return RedirectToPage("Index");
    }
}
