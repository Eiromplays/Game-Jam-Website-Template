using GameJam.Api.Authorize;
using GameJam.Api.Interfaces;
using GameJam.Api.Models;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Differencing;

namespace GameJam.Areas.Identity.Pages.Account.Manage
{
    [AuthorizeAdminAttribute]
    public class UploadedGamesModel : PageModel
    {
        private readonly UserManager<GameJamUser> _userManager;
        private readonly IGameRepository _gameRepository;


        public UploadedGamesModel(UserManager<GameJamUser> userManager, IGameRepository gameRepository)
        {
            _userManager = userManager;
            _gameRepository = gameRepository;
        }

        public List<Game> Games { get; set; } = new List<Game>();

        public Game EditGame { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string GameId { get; set; }

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

            [Display(Name = "Images (Separate with ,):")]
            public string Images { get; set; }

            [Display(Name = "Videos (Separate with ,):")]
            public string Videos { get; set; }

            [Required]
            public bool Approved { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string editGameId, string deleteGameId, int deleteRating)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (deleteRating > 0)
            {
                var result = await _gameRepository.RemoveRatingAsync(deleteRating);
                StatusMessage = result.Succeeded ? "Successfully removed rating." : "Error while removing rating.";

                return RedirectToPage();
            }

            if (!string.IsNullOrEmpty(deleteGameId))
            {
                var result = await _gameRepository.RemoveGameAsync(deleteGameId);

                if (result.Succeeded)
                {
                    StatusMessage = "Successfully removed game.";
                    return RedirectToPage();
                }

                if (result.IsNotFound)
                {
                    StatusMessage = "Error game not found.";
                    return RedirectToPage();
                }

                if (result.Failed)
                {
                    StatusMessage = "Error while deleting game.";
                }

                return RedirectToPage();
            }

            if (string.IsNullOrEmpty(editGameId))
            {
                Games = await _gameRepository.GetGamesAsync();

                await GetPublisherInformationAsync();

                return Page();
            }

            EditGame = await _gameRepository.GetGameAsync(editGameId);

            if (EditGame == null) return Page();

            await GetPublisherInformationAsync();

            Input = new InputModel
            {
                GameId = EditGame.Id,
                Name = EditGame.Name,
                Description = EditGame.Description,
                DownloadLink = EditGame.DownloadLink,
                Images = string.Join(",", EditGame.Images),
                Videos = string.Join(",", EditGame.Videos),
                Approved = EditGame.Approved
            };

            return Page();
        }

        public async Task<IActionResult> OnPostEditGameAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid) return Page();

            EditGame = await _gameRepository.GetGameAsync(Input.GameId);

            if (EditGame == null)
            {
                StatusMessage = "Error Game not found";
                return RedirectToPage();
            }

            EditGame.Description = Input.Description;
            EditGame.Name = Input.Name;
            EditGame.DownloadLink = Input.DownloadLink;
            EditGame.Images = Input.Images != null ? Input.Images.Split(",").ToList() : new List<string>();
            EditGame.Videos = Input.Videos != null ? Input.Videos.Split(",").ToList() : new List<string>();
            EditGame.Approved = Input.Approved;

            var result = await _gameRepository.UpdateGameAsync(EditGame);

            StatusMessage = result.Succeeded ? "Successfully updated game." : "Error while updating game.";

            return RedirectToPage(new {editGameId = Input.GameId});
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
            return Content(Url.Page("./MyGames", new { editGameId = deleteModel.GameId }));
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
