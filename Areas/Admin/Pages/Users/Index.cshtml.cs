using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace InvApp.Areas.Admin.Pages.Users;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userMgr;
    private readonly RoleManager<IdentityRole> _roleMgr;
    public IndexModel(UserManager<IdentityUser> userMgr, RoleManager<IdentityRole> roleMgr)
    {
        _userMgr = userMgr; _roleMgr = roleMgr;
    }

    public record Row(string Id, string Email, string UserName, List<string> Roles);
    public List<Row> Users { get; set; } = new();

    public async Task OnGetAsync()
    {
        var list = await _userMgr.Users.AsNoTracking().ToListAsync();
        var rows = new List<Row>();
        foreach (var u in list)
        {
            var roles = (await _userMgr.GetRolesAsync(u)).ToList();
            rows.Add(new Row(u.Id, u.Email ?? "", u.UserName ?? "", roles));
        }
        Users = rows.OrderBy(r => r.Email).ToList();
    }
}
