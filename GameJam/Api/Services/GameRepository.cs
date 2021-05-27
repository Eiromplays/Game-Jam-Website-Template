using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameJam.Api.Interfaces;
using GameJam.Api.Models;
using GameJam.Api.Results;
using GameJam.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GameJam.Api.Services
{
    public class GameRepository : IGameRepository
    {
        private readonly GameJamDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public GameRepository(GameJamDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        /// <summary>
        /// Create/Add a new game.
        /// </summary>
        public async Task<GameResult> AddGameAsync(Game game)
        {
            var maxSubmissions = _configuration.GetValue<int>("MaxSubmissionsPerUser");
            if (maxSubmissions <= 0) return GameResult.MaxSubmissionsNotSet;

            if((await GetUsersGameAsync(game.PublisherUserId)).Count > maxSubmissions)
                return GameResult.MaxSubmissions;

            await _dbContext.Games.AddAsync(game);

            return await _dbContext.SaveChangesAsync() >= 1 ? GameResult.Success : GameResult.Failed;
        }
        /// <summary>
        /// Updates a preexisting game
        /// </summary>

        public async Task<GameResult> UpdateGameAsync(Game game)
        {
            _dbContext.Games.Update(game);

            return await _dbContext.SaveChangesAsync() >= 1 ? GameResult.Success : GameResult.Failed;
        }

        /// <summary>
        /// Remove a game that exists using it's id.
        /// </summary>

        public async Task<GameResult> RemoveGameAsync(string gameId)
        {
            var gameToRemove = await GetGameAsync(gameId);

            if(gameToRemove == null) return GameResult.NotFound;

            _dbContext.Games.Remove(gameToRemove);

            return await _dbContext.SaveChangesAsync() >= 1 ? GameResult.Success : GameResult.Failed;
        }

        public async Task<GameResult> RemoveGameAsync(string gameId, string userId)
        {
            var gameToRemove = await GetGameAsync(gameId);

            if (gameToRemove == null) return GameResult.NotFound;
            if(!gameToRemove.PublisherUserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
                return GameResult.Failed;

            _dbContext.Games.Remove(gameToRemove);

            return await _dbContext.SaveChangesAsync() >= 1 ? GameResult.Success : GameResult.Failed;
        }

        /// <summary>
        /// Get a game using it's id.
        /// </summary>
        public async Task<Game> GetGameAsync(string gameId)
        {
            return await _dbContext.Games.FirstOrDefaultAsync(g =>
                g.Id == gameId);
        }

        /// <summary>
        /// Gives a list of games with the name you specified
        /// </summary>
        public async Task<List<Game>> GetGamesByNameAsync(string gameName)
        {
            return (await _dbContext.Games.ToListAsync()).Where(g =>
                g.Name.Equals(gameName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Gives a list of games a specific user has uploaded.
        /// </summary>
        public async Task<List<Game>> GetUsersGameAsync(string userId)
        {
            return (await _dbContext.Games.ToListAsync()).Where(g =>
                g.PublisherUserId.Equals(userId, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Gives a list of all games.
        /// </summary>
        public async Task<List<Game>> GetGamesAsync()
        {
            return await _dbContext.Games.ToListAsync();
        }

        /// <summary>
        /// Gives you a list of all games that are approved.
        /// </summary>
        public async Task<List<Game>> GetApprovedGamesAsync()
        {
            return (await _dbContext.Games.ToListAsync()).Where(g => g.Approved).ToList();
        }

        /// <summary>
        /// Gives you a list of all games that are not approved.
        /// </summary>
        public async Task<List<Game>> GetNotApprovedGamesAsync()
        {
            return (await _dbContext.Games.ToListAsync()).Where(g => !g.Approved).ToList();
        }
    }
}
