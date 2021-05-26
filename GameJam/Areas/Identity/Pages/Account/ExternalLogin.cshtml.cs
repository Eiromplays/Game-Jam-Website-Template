using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Discord;
using GameJam.Api.Models;
using Microsoft.AspNetCore.Authorization;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GameJam.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<GameJamUser> _signInManager;
        private readonly UserManager<GameJamUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly IConfiguration _configuration;

        public ExternalLoginModel(
            SignInManager<GameJamUser> signInManager,
            UserManager<GameJamUser> userManager,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender, 
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Username { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        private readonly List<string> _profilePictureClaims = new List<string>
        {
            "urn:google:profile", DiscordAuthenticationConstants.Claims.AvatarUrl
        };

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor : true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);

                await UpdateProfilePictureAsync(info);

                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            if (result.IsNotAllowed)
            {
                return RedirectToPage("./NotAllowed");
            }

            // If the user does not have an account, then ask the user to create an account.
            ReturnUrl = returnUrl;
            ProviderDisplayName = info.ProviderDisplayName;
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email) && 
                info.Principal.HasClaim(c => c.Type == ClaimTypes.Name))
            {
                Input = new InputModel
                {
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                    Username = info.Principal.FindFirstValue(ClaimTypes.Name)
                };
            }

            return Page();
        }

        private async Task UpdateProfilePictureAsync(ExternalLoginInfo info)
        {
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user == null) return;

            var profilePicture = await GetProfilePictureAsync(info);

            if (user.ProfilePicture.Equals(profilePicture, StringComparison.OrdinalIgnoreCase)) return;

            user.ProfilePicture = profilePicture;

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Updated the profile picture for {Id}", user.Id);
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var profilePicture = await GetProfilePictureAsync(info);

                var user = new GameJamUser { UserName = Input.Username, Email = Input.Email, ProfilePicture = profilePicture };

                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account with the name {user.UserName} by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation");
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private async Task<string> GetProfilePictureAsync(ExternalLoginInfo info)
        {
            var profilePicture = "";

            foreach (var claim in info.Principal.Claims)
            {
                // Checks to see if the avatar hash for discord is set or not.
                if (claim.Type.Equals(DiscordAuthenticationConstants.Claims.AvatarUrl,
                    StringComparison.OrdinalIgnoreCase) && !info.Principal.HasClaim(c =>
                    c.Type.Equals(DiscordAuthenticationConstants.Claims.AvatarHash,
                        StringComparison.OrdinalIgnoreCase))) continue;

                if (_profilePictureClaims.Contains(claim.Type))
                    profilePicture = claim.Value;
            }

            /* If the user tries to login with google it will download the profile picture using google's api. (Requires a API key for google's people API) */
            if (info.LoginProvider.ToLower() != "google") return profilePicture;

            try
            {
                var httpClient = new HttpClient();

                string peopleApiKey = _configuration["GoogleApiKeys:PeopleApiKey"];
                var googleAccountId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var photosResponse = await httpClient.GetFromJsonAsync<PeopleApiPhotos>(
                    $"https://people.googleapis.com/v1/people/{googleAccountId}?personFields=photos&key={peopleApiKey}");
                profilePicture = photosResponse?.photos.FirstOrDefault()?.url;
            }
            // If there is any exceptions they will be logged in console
            catch (Exception ex)
            {
                Console.WriteLine($"Error while getting profile picture: \n {ex}");
            }

            return profilePicture;
        }
    }
}
