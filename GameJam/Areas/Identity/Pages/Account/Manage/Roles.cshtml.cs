using System;
using GameJam.Api.Authorize;
using GameJam.Api.Models;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GameJam.Areas.Identity.Pages.Account.Manage
{
    [AuthorizeAdminAttribute]
    public class RolesModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<GameJamUser> _userManager;
        private readonly DefaultRoleNames _defaultRoleNames;

        public RolesModel(RoleManager<IdentityRole> roleManager, 
            UserManager<GameJamUser> userManager,
            IOptions<DefaultRoleNames> defaultRoleNames)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _defaultRoleNames = defaultRoleNames.Value;
        }

        public List<IdentityRole> Roles { get; set; } = new List<IdentityRole>();

        [TempData]
        public string StatusMessage { get; set; }

        public bool CreateRole { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Role Name:")]
            public string Name { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string identifier, string deleteRoleId, bool createRole)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (createRole)
            {
                CreateRole = true;
                return Page();
            }

            if (!string.IsNullOrEmpty(deleteRoleId))
            {
                var deleteRole = await _roleManager.FindByIdAsync(deleteRoleId);
                if (deleteRole == null)
                {
                    StatusMessage = "Error role not found.";
                    return RedirectToPage();
                }

                if (deleteRole.Name.Equals(_defaultRoleNames.AdministratorRoleName,
                    StringComparison.OrdinalIgnoreCase) || deleteRole.Name.Equals(_defaultRoleNames.ParticipantRoleName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    StatusMessage = "Error That role cannot be removed, as it is required for the website to work.";
                    return RedirectToPage();
                }

                var result = await _roleManager.DeleteAsync(deleteRole);

                StatusMessage = result.Succeeded ? "Successfully removed role" : "Error while removing role.";

                return RedirectToPage();
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

        public async Task<IActionResult> OnPostCreateRoleAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid) return Page();

            var newRole = new IdentityRole(Input.Name);

            var result = await _roleManager.CreateAsync(newRole);

            StatusMessage = result.Succeeded ? "Successfully created role" : "Error unable to create new role";

            return RedirectToPage();
        }
    }
}
