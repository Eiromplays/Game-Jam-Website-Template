using System;
using GameJam.Api.Authorize;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GameJam.Api.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace GameJam.Areas.Identity.Pages.Account.Manage
{
    [AuthorizeAdminAttribute]
    public class UsersModel : PageModel
    {
        private readonly UserManager<GameJamUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly DefaultRoleNames _defaultRoleNames;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersModel(UserManager<GameJamUser> userManager, IConfiguration configuration,
            IOptions<DefaultRoleNames> defaultRoleNames, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _defaultRoleNames = defaultRoleNames.Value;
        }

        public List<GameJamUser> Users { get; set; } = new List<GameJamUser>();

        public GameJamUser EditUser { get; set; }

        public List<SelectListItem> AddRoles { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> RemoveRoles { get; set; } = new List<SelectListItem>();

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public string AddRole { get; set; }

        [BindProperty]
        public string RemoveRole { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string UserId { get; set; }

            [Required]
            public bool EmailConfirmed { get; set; }

            [Required]
            public string Username { get; set; }

            public string Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string identifier, string deleteUserId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!string.IsNullOrEmpty(deleteUserId))
            {
                var deleteUser = await _userManager.FindByIdAsync(deleteUserId);
                if (deleteUser == null)
                {
                    StatusMessage = $"Error User with Id: {deleteUserId} was not found";
                    return RedirectToPage();
                }

                if (deleteUser.UserName.Equals(_configuration["AdminUsername"], StringComparison.OrdinalIgnoreCase))
                {
                    StatusMessage = "Error This is the main admin account so it cannot be deleted.";
                    return RedirectToPage();
                }

                var result = await _userManager.DeleteAsync(deleteUser);

                if (!result.Succeeded)
                {
                    StatusMessage = "Error unable to delete user.";
                    return RedirectToPage();
                }

                StatusMessage = "Successfully deleted user.";
                return RedirectToPage();
            }

            if (string.IsNullOrEmpty(identifier))
            {
                Users = await _userManager.Users.ToListAsync();
                return Page();
            }

            EditUser = await _userManager.FindByIdAsync(identifier) ?? 
                       await _userManager.FindByNameAsync(identifier);

            if (EditUser == null) return Page();

            Input = new InputModel
            {
                UserId = EditUser.Id,
                EmailConfirmed = EditUser.EmailConfirmed,
                Description = EditUser.Description,
                Username = EditUser.UserName
            };

            AddRoles.Add(new SelectListItem("None", "None", true));
            RemoveRoles.Add(new SelectListItem("None", "None", true));

            var removeRoles = await _userManager.GetRolesAsync(EditUser);
            var addRoles = (await _roleManager.Roles.ToListAsync()).Where(r => !removeRoles.Contains(r.Name)).ToList();

            AddRoles.AddRange(new SelectList(addRoles, nameof(IdentityRole.Name),
                nameof(IdentityRole.Name)).ToList());
            AddRoles = AddRoles.Distinct().ToList();

            RemoveRoles.AddRange(new SelectList(removeRoles));

            return Page();
        }

        public async Task<IActionResult> OnPostEditUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid) return Page();

            EditUser = await _userManager.FindByIdAsync(Input.UserId);
            if (EditUser == null)
            {
                StatusMessage = "Error user was not found.";
                return RedirectToPage();
            }

            if(!await UpdateRemoveRoleAsync()) 
                return RedirectToPage(new { identifier = EditUser.Id });

            if (!await UpdateAddRoleAsync())
            {
                StatusMessage = "Error while adding user to role.";
                return RedirectToPage(new {identifier = EditUser.Id});
            }

            EditUser.Description = Input.Description ?? "";
            EditUser.UserName = Input.Username;

            EditUser.EmailConfirmed = Input.EmailConfirmed;
            if (await _userManager.UpdateAsync(EditUser) != IdentityResult.Success)
            {
                StatusMessage = "Error while updating user.";
                return Page();
            }

            StatusMessage = "Successfully edited user.";

            return RedirectToPage(new {identifier = EditUser.Id});
        }

        private async Task<bool> UpdateAddRoleAsync()
        {
            if (!AddRole.Equals("None", StringComparison.OrdinalIgnoreCase) && 
                !await _userManager.IsInRoleAsync(EditUser, AddRole))
            {
                return await _userManager.AddToRoleAsync(EditUser, AddRole) == IdentityResult.Success;
            }

            return true;
        }

        private async Task<bool> UpdateRemoveRoleAsync()
        {
            if (RemoveRole.Equals("None", StringComparison.OrdinalIgnoreCase)) return true;

            if (EditUser.UserName.Equals(_configuration["AdminUsername"], StringComparison.OrdinalIgnoreCase) &&
                RemoveRole.Equals(_defaultRoleNames.AdministratorRoleName, StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = "Error This is the main admin account so the admin role can't be removed.";
                return false;
            }

            if (!RemoveRole.Equals("None", StringComparison.OrdinalIgnoreCase) &&
                await _userManager.IsInRoleAsync(EditUser, RemoveRole))
            {
                return await _userManager.RemoveFromRoleAsync(EditUser, RemoveRole) == IdentityResult.Success;
            }

            return true;
        }
    }
}
