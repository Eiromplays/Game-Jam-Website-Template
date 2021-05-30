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

        /// <summary>
        /// Updates a preexisting game
        /// </summary>
        Task<GameJam.Api.Results.GameResult> UpdateGameAsync(Game game);

        /// <summary>
        /// Remove a image from a game.
        /// </summary>
        Task<GameJam.Api.Results.GameResult> RemoveImageAsync(string gameId, string image);

        /// <summary>
        /// Remove a video from a game.
        /// </summary>
        Task<GameJam.Api.Results.GameResult> RemoveVideoAsync(string gameId, string video);

        /// <summary>
        /// Create/Add a new game.
        /// </summary>
        Task<GameJam.Api.Results.GameResult> AddGameAsync(Game game);

        /// <summary>
        /// Create/Add a new rating to a game.
        /// </summary>
        Task<GameJam.Api.Results.GameResult> AddRatingAsync(string gameId, string userId, float rating);

        /// <summary>
        /// Remove a new rating from a game.
        /// </summary>
        Task<GameJam.Api.Results.GameResult> RemoveRatingAsync(string gameId, string userId);

        /// <summary>
        /// Get a users rating for a game.
        /// </summary>
        Task<GameRating> GetRatingAsync(string gameId, string userId);

        Task<List<GameRating>> GetGameRatingsAsync(string gameId);
        Task<GameJam.Api.Results.GameResult> RemoveRatingAsync(int id);
    }
}
