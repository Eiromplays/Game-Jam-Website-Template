using GameJam.Api.Authorize;
using GameJam.Api.Models;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameJam.Areas.Identity.Pages.Account.Manage
{
    [AuthorizeAdminAttribute]
    public class RolesModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<GameJamUser> _userManager;
        private readonly IOptions<DefaultRoleNames> _roleNames;

        public RolesModel(RoleManager<IdentityRole> roleManager, 
            UserManager<GameJamUser> userManager,
            IOptions<DefaultRoleNames> roleNames)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _roleNames = roleNames;
        }

        public List<IdentityRole> Roles = new List<IdentityRole>();

        public async Task<IActionResult> OnGetAsync(string identifier)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (string.IsNullOrEmpty(identifier))
            {
                Roles = await _roleManager.Roles.ToListAsync();
                return Page();
            }

            var role = await _roleManager.FindByIdAsync(identifier) ?? 
                       await _roleManager.FindByNameAsync(identifier);

            Roles.Add(role);

            return Page();
        }

    }
}
