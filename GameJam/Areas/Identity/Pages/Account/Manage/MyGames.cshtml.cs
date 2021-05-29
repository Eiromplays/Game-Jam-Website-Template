using GameJam.Api.Authorize;
using GameJam.Api.Interfaces;
using GameJam.Api.Models;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GameJam.Areas.Identity.Pages.Account.Manage
{
    [AuthorizeParticipant]
    public class MyGames : PageModel
    {
        private readonly IGameRepository _gameRepository;
        private readonly UserManager<GameJamUser> _userManager;

        public MyGames(IGameRepository gameRepository, 
            UserManager<GameJamUser> userManager)
        {
            _gameRepository = gameRepository;
            _userManager = userManager;
        }

        public List<Game> Games { get; set; } = new List<Game>();

        public Game EditGame { get; set; }

        public List<string> GamesThatCanBeEdited { get; set; } = new List<string>();

        public bool UploadNewGame { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Your games name:")]
            public string Name { get; set; }

            [Required]
            [Display(Name = "Description:")]
            public string Description { get; set; }

            [Required]
            [Display(Name = "Download link:")]
            [Url]
            public string DownloadLink { get; set; }

            public string GameId { get; set; }

            [Display(Name = "Images (Separate with ,):")]
            public string Images { get; set; }

            [Display(Name = "Videos (Separate with ,):")]
            public string Videos { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string deleteGameId, string editGameId, bool uploadGame)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (IsUploadGame(uploadGame, out var actionResult)) return actionResult;

            if (await CheckForDeleteAsync(deleteGameId, user)) return RedirectToPage();

            if (await CheckForEditGameAsync(editGameId, user)) return Page();

            Games = await _gameRepository.GetUsersGameAsync(user.Id);

            GetGamesUserCanEdit(user.Id);

            await GetPublisherInformationAsync();

            return Page();
        }

        private bool IsUploadGame(bool uploadGame, out IActionResult actionResult)
        {
            if (!uploadGame)
            {
                actionResult = Page();
                return false;
            }

            UploadNewGame = true;

            actionResult = Page();
            return true;
        }

        private async Task<bool> CheckForDeleteAsync(string deleteGameId, GameJamUser user)
        {
            if (string.IsNullOrEmpty(deleteGameId)) return false;
            var result = await _gameRepository.RemoveGameAsync(deleteGameId, user.Id);

            if (!result.Succeeded)
            {
                return true;
            }

            StatusMessage = "Successfully removed Game.";

            return true;
        }

        private async Task<bool> CheckForEditGameAsync(string editGameId, GameJamUser user)
        {
            if (string.IsNullOrEmpty(editGameId)) return false;
            EditGame = await _gameRepository.GetGameAsync(editGameId);

            if (EditGame == null || !EditGame.PublisherUserId.Equals(user.Id)) return false;

            await GetPublisherInformationAsync();

            Input = new InputModel
            {
                GameId = EditGame.Id,
                Name = EditGame.Name,
                Description = EditGame.Description,
                DownloadLink = EditGame.DownloadLink,
                Images = string.Join(",", EditGame.Images),
                Videos = string.Join(",", EditGame.Videos)
            };

            return true;
        }

        public async Task GetPublisherInformationAsync()
        {
            foreach (var game in Games)
            {
                var publisher = await _userManager.FindByIdAsync(game.PublisherUserId).ConfigureAwait(false);

                if (publisher == null)
                {
                    game.PublisherUserId = $"Unable to load your username.";
                    continue;
                }

                game.PublisherUserId = publisher.UserName;
            }

            if (EditGame != null)
            {
                var publisher = await _userManager.FindByIdAsync(EditGame.PublisherUserId).ConfigureAwait(false);

                if (publisher == null)
                {
                    EditGame.PublisherUserId = $"Unable to load your username.";
                    return;
                }

                EditGame.PublisherUserId = publisher.UserName;
            }
        }

        public void GetGamesUserCanEdit(string userId)
        {
            GamesThatCanBeEdited.AddRange(Games.Where(g => g.PublisherUserId == userId).Select(g => g.Id));
        }

        public async Task<IActionResult> OnPostEditGameAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid || Input.GameId == null) return Page();

            var game = await _gameRepository.GetGameAsync(Input.GameId);
            if (game == null)
            {
                StatusMessage = "Error Game not found.";
                return RedirectToPage();
            }

            game.Description = Input.Description;
            game.Name = Input.Name;
            game.DownloadLink = Input.DownloadLink;
            game.Images = Input.Images != null ? Input.Images.Split(",").ToList() : new List<string>();
            game.Videos = Input.Videos != null ? Input.Videos.Split(",").ToList() : new List<string>();

            var result = await _gameRepository.UpdateGameAsync(game);

            if (!result.Succeeded)
            {
                StatusMessage = "Error failed to updated game.";
                return RedirectToPage(new {editGameId = game.Id});
            }

            StatusMessage = "Successfully updated game.";
            return RedirectToPage(new {editGameId = game.Id});

        }

        public async Task<IActionResult> OnPostUploadGameAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid) return RedirectToPage();

            var game = new Game(Input.Name, Input.Description, user.Id, Input.DownloadLink,
                Input.Images?.Split(",").ToList() ?? new List<string>(),
                Input.Videos?.Split(",").ToList() ?? new List<string>(), false);

            var result = await _gameRepository.AddGameAsync(game);

            if (result.Succeeded)
            {
                StatusMessage = "Successfully created game";
                return RedirectToPage();
            }

            if (!result.HasMaxSubmissions)
            {
                StatusMessage = "Error occurred while adding game.";
                return RedirectToPage();
            }

            StatusMessage = "Error adding game, you have reached the maximum allowed submissions.";
            return RedirectToPage();

        }

        public async Task<IActionResult> OnPostDeletePictureAsync(DeleteModel deleteModel)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || deleteModel == null)
            {
                return Content(Url.Page("./MyGames"));
            }

            var result = await _gameRepository.RemoveImageAsync(deleteModel.GameId, deleteModel.Value);

            if (!result.Succeeded)
            {
                StatusMessage = "Unable to delete image.";
                return Content(Url.Page("./MyGames", new { editGameId = deleteModel.GameId }));
            }

            StatusMessage = "Successfully deleted image.";
            return Content(Url.Page("./MyGames", new {editGameId = deleteModel.GameId}));
        }

        public async Task<IActionResult> OnPostDeleteVideoAsync(DeleteModel deleteModel)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || deleteModel == null)
            {
                return Content(Url.Page("./MyGames"));
            }

            var result = await _gameRepository.RemoveVideoAsync(deleteModel.GameId, deleteModel.Value);

            if (!result.Succeeded)
            {
                StatusMessage = "Unable to delete image.";
                return Content(Url.Page("./MyGames", new { editGameId = deleteModel.GameId }));
            }

            StatusMessage = "Successfully deleted video.";
            return Content(Url.Page("./MyGames", new { editGameId = deleteModel.GameId }));
        }
    }
}
