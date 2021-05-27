using System.Collections.Generic;
using System.Threading.Tasks;
using GameJam.Api.Models;

namespace GameJam.Api.Interfaces
{
    public interface IGameRepository
    {
        /// <summary>
        /// Gives a list of all games.
        /// </summary>
        Task<List<Game>> GetGamesAsync();

        /// <summary>
        /// Get a game using it's id.
        /// </summary>
        Task<Game> GetGameAsync(string gameId);

        /// <summary>
        /// Gives a list of games with the name you specified
        /// </summary>
        Task<List<Game>> GetGamesByNameAsync(string gameName);

        /// <summary>
        /// Gives a list of games a specific user has uploaded.
        /// </summary>
        Task<List<Game>> GetUsersGameAsync(string userId);

        /// <summary>
        /// Gives you a list of all games that are approved.
        /// </summary>
        Task<List<Game>> GetApprovedGamesAsync();

        /// <summary>
        /// Gives you a list of all games that are not approved.
        /// </summary>
        Task<List<Game>> GetNotApprovedGamesAsync();

        Task<GameJam.Api.Results.GameResult> RemoveGameAsync(string gameId);
        Task<GameJam.Api.Results.GameResult> RemoveGameAsync(string gameId, string userId);
    }
}
