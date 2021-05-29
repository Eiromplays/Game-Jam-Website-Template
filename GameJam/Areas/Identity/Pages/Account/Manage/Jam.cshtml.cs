using GameJam.Api.Models;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace GameJam.Areas.Identity.Pages.Account.Manage
{
    public class JamModel : PageModel
    {
        private readonly UserManager<GameJamUser> _userManager;
        private readonly DefaultRoleNames _defaultRoleNames;

        public JamModel(
            UserManager<GameJamUser> userManager,
            IOptions<DefaultRoleNames> defaultRoleNames)
        {
            _userManager = userManager;
            _defaultRoleNames = defaultRoleNames.Value;
        }

        public bool Participant { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Participant = await _userManager.IsInRoleAsync(user, _defaultRoleNames.ParticipantRoleName);

            if (Participant)
            {
                var leaveResult = await _userManager.RemoveFromRoleAsync(user, _defaultRoleNames.ParticipantRoleName);
                StatusMessage = leaveResult.Succeeded ? "You successfully left the KTF Game Jam" : "Error unable to leave KTF Game Jam";

                return Redirect(Request.Headers["Referer"].ToString());
            }

            var joinResult = await _userManager.AddToRoleAsync(user, _defaultRoleNames.ParticipantRoleName);
            StatusMessage = joinResult.Succeeded ? "You successfully joined the KTF Game Jam" : "Error unable to join KTF Game Jam";

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
