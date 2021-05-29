using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameJam.Api.Models;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GameJam.Pages.Home
{
    public class ViewUserModel : PageModel
    {
        private readonly UserManager<GameJamUser> _userManager;
        private readonly DefaultRoleNames _defaultRoleNames;

        public ViewUserModel(UserManager<GameJamUser> userManager, 
            IOptions<DefaultRoleNames> defaultRoleNames)
        {
            _userManager = userManager;
            _defaultRoleNames = defaultRoleNames.Value;
        }

        public string Query { get; set; }

        public List<GameJamUser> Users { get; }= new List<GameJamUser>();

        public List<string> StaffIds { get; set; } = new List<string>();

        public async Task<IActionResult> OnGetAsync(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return Page();
            }

            Query = query;

            var foundUser = await _userManager.FindByIdAsync(query) ?? await _userManager.FindByNameAsync(query);

            if(foundUser != null)
                Users.Add(foundUser);
            else
            {
                Users.AddRange((await _userManager.Users.ToListAsync()).Where(u =>
                    u.Email.Equals(query, StringComparison.OrdinalIgnoreCase)).ToList());
            }

            foreach (var user in Users)
            {
                if(await _userManager.IsInRoleAsync(user, _defaultRoleNames.AdministratorRoleName))
                    StaffIds.Add(user.Id);
            }

            return Page();
        }
    }
}
