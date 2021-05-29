using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameJam.Api.Models;
using Microsoft.Extensions.Options;

namespace GameJam.Pages.Home
{
    public class AboutModel : PageModel
    {
        private readonly UserManager<GameJamUser> _userManager;
        private readonly DefaultRoleNames _defaultRoleNames;

        public AboutModel(UserManager<GameJamUser> userManager, 
            IOptions<DefaultRoleNames> defaultRoleNames)
        {
            _userManager = userManager;
            _defaultRoleNames = defaultRoleNames.Value;
        }

        public List<GameJamUser> StaffUsers { get; set; }= new List<GameJamUser>();

        public async Task OnGetAsync()
        {
            foreach (var user in await _userManager.Users.ToListAsync())
            {
                if(await _userManager.IsInRoleAsync(user, _defaultRoleNames.AdministratorRoleName))
                    StaffUsers.Add(user);
            }
        }

    }
}
