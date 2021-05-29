using System.ComponentModel.DataAnnotations;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using GameJam.Api.Models;
using Microsoft.Extensions.Options;

namespace GameJam.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<GameJamUser> _userManager;

        public IndexModel(
            UserManager<GameJamUser> userManager)
        {
            _userManager = userManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Username = await _userManager.GetUserNameAsync(user);

            Input = new InputModel
            {
                Description = user.Description
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid || string.IsNullOrEmpty(Input.Description)) return Page();

            user.Description = Input.Description;

            var result = await _userManager.UpdateAsync(user);

            StatusMessage = result.Succeeded ? "Successfully updated your description." : "Error failed to updated your description.";

            return RedirectToPage();
        }
    }
}
