using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameJam.Pages.Home
{
    public class OmModel : PageModel
    {
        private readonly UserManager<GameJamUser> _userManager;

        public OmModel(UserManager<GameJamUser> userManager)
        {
            _userManager = userManager;
        }

        public List<GameJamUser> Users = new List<GameJamUser>();

        public async Task OnGetAsync()
        {
            Users = (await _userManager.Users.ToListAsync());
        }
    }
}
