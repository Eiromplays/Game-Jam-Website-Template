using GameJam.Api.Interfaces;
using GameJam.Api.Models;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameJam.Pages.Home
{
    public class GamesModel : PageModel
    {
        private readonly IGameRepository _gameRepository;
        private readonly UserManager<GameJamUser> _userManager;
        private readonly DefaultRoleNames _defaultRoleNames;

        public GamesModel(IGameRepository gameRepository, 
            UserManager<GameJamUser> userManager, 
            IOptions<DefaultRoleNames> defaultRoleNames)
        {
            _gameRepository = gameRepository;
            _userManager = userManager;
            _defaultRoleNames = defaultRoleNames.Value;
        }

        public List<Game> Games { get; set; } = new List<Game>();

        public List<string> GamesThatCanBeEdited { get; set; } = new List<string>();

        public List<string> GamesThatCanBeEditedAdmin { get; set; } = new List<string>();

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

                await GetGamesUserCanEditAsync(user);

                await GetPublisherInformationAsync();

                return Page();
            }

            var selectedGame = await _gameRepository.GetGameAsync(query);
            if (selectedGame != null)
            {
                Games.Add(selectedGame);

                await GetGamesUserCanEditAsync(user);

                await GetPublisherInformationAsync();

                return Page();
            }

            Games.AddRange(await _gameRepository.GetGamesByNameAsync(query));
            Games.AddRange(await _gameRepository.GetUsersGameAsync(query));

            // Removes duplicate objects if any.
            Games = Games.Distinct().ToList();

            await GetGamesUserCanEditAsync(user);

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
        }

        public async Task GetGamesUserCanEditAsync(GameJamUser user)
        {
            GamesThatCanBeEdited.AddRange(Games.Where(g => g.PublisherUserId == user.Id).Select(g => g.Id).ToList());
            if (await _userManager.IsInRoleAsync(user, _defaultRoleNames.AdministratorRoleName))
            {
                GamesThatCanBeEditedAdmin = Games.Select(g => g.Id).ToList();
            }
        }
    }
}
