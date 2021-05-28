using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GameJam.Api.Authorize;
using GameJam.Api.Interfaces;
using GameJam.Api.Models;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

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

        public List<Game> Games = new List<Game>();

        public Game EditGame;

        public List<string> GamesThatCanBeEdited = new List<string>();

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

            [Required]
            public string GameId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string deleteGameId, string editGameId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!string.IsNullOrEmpty(deleteGameId))
            {
                var result = await _gameRepository.RemoveGameAsync(deleteGameId, user.Id);

                if (result.Succeeded)
                {
                    Console.WriteLine($"Successfully removed Game");
                    return RedirectToPage();
                }

                Console.WriteLine($"Result {JsonConvert.SerializeObject(result)}");

                return RedirectToPage();
            }

            if (!string.IsNullOrEmpty(editGameId))
            {
                EditGame = await _gameRepository.GetGameAsync(editGameId);
                if (EditGame != null && EditGame.PublisherUserId.Equals(user.Id))
                {
                    await GetPublisherInformationAsync();

                    Input = new InputModel
                    {
                        GameId = EditGame.Id,
                        Name = EditGame.Name,
                        Description = EditGame.Description,
                        DownloadLink = EditGame.DownloadLink
                    };

                    return Page();
                }
            }

            Games = await _gameRepository.GetUsersGameAsync(user.Id);

            GetGamesUserCanEdit(user.Id);

            await GetPublisherInformationAsync();

            return Page();
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
            if (!ModelState.IsValid)
            {
                
                return Page();
            }

            var game = await _gameRepository.GetGameAsync(Input.GameId);
            if (game == null)
            {
                StatusMessage = "Error Game not found";
                return RedirectToPage();
            }

            game.Description = Input.Description;
            game.Name = Input.Name;
            game.DownloadLink = Input.DownloadLink;

            var result = await _gameRepository.UpdateGameAsync(game);

            if (result.Succeeded)
            {
                StatusMessage = "Successfully updated game";
                return RedirectToPage(new {editGameId = game.Id});
            }

            return RedirectToPage(new { editGameId  = game.Id});
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