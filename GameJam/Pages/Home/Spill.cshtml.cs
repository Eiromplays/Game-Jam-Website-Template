using System;
using GameJam.Api.Interfaces;
using GameJam.Api.Models;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GameJam.Pages.Home
{
    public class SpillModel : PageModel
    {
        private readonly IGameRepository _gameRepository;
        private readonly UserManager<GameJamUser> _userManager;

        public SpillModel(IGameRepository gameRepository, 
            UserManager<GameJamUser> userManager)
        {
            _gameRepository = gameRepository;
            _userManager = userManager;
        }

        public List<Game> Games = new List<Game>();

        /// <summary>
        /// Loads games depending on if the user tries to query or not.
        /// </summary>
        public async Task<IActionResult> OnGetAsync(string query)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (string.IsNullOrEmpty(query))
            {
                Games = await _gameRepository.GetApprovedGamesAsync();
                await GetPublisherInformationAsync();

                return Page();
            }

            var selectedGame = await _gameRepository.GetGameAsync(query);
            if (selectedGame != null)
            {
                Games.Add(selectedGame);

                await GetPublisherInformationAsync();

                return Page();
            }

            Games.AddRange(await _gameRepository.GetGamesByNameAsync(query));
            Games.AddRange(await _gameRepository.GetUsersGameAsync(query));

            // Removes duplicate objects if any.
            Games = Games.Distinct().ToList();

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
                    game.PublisherUserId = $"Unable to load publisher information.";
                    continue;
                }

                game.PublisherUserId = publisher.UserName;
            }
        }
    }
}
