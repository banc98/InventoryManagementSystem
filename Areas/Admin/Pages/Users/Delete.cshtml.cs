using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InvApp.Areas.Admin.Pages.Users;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly UserManager<IdentityUser> _userMgr;
    public DeleteModel(UserManager<IdentityUser> userMgr) => _userMgr = userMgr;

    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var user = await _userMgr.FindByIdAsync(id);
        if (user is null) return NotFound();
        Id = user.Id; Email = user.Email ?? "";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        var user = await _userMgr.FindByIdAsync(id);
        if (user is null) return RedirectToPage("Index");

        // 1) Don't let a user delete themselves
        var currentUserId = _userMgr.GetUserId(User);
        if (user.Id == currentUserId)
        {
            TempData["FlashError"] = "You can’t delete the account you’re currently signed in with.";
            return RedirectToPage("Index");
        }

        // 2) Ensure at least one Admin remains
        if (await _userMgr.IsInRoleAsync(user, "Admin"))
        {
            var admins = await _userMgr.GetUsersInRoleAsync("Admin");
            if (admins.Count <= 1)
            {
                TempData["FlashError"] = "Blocked: at least one Admin account must remain.";
                return RedirectToPage("Index");
            }
        }

        // 3) Delete
        var result = await _userMgr.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var message = string.Join("; ", result.Errors.Select(e => e.Description));
            TempData["FlashError"] = $"Unable to delete user: {message}";
            return RedirectToPage("Index");
        }

        TempData["FlashSuccess"] = $"Deleted user {user.Email}.";
        return RedirectToPage("Index");
    }
}
