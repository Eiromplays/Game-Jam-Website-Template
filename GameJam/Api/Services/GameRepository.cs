using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GameJam.Api.Extensions;
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

            if((await GetUsersGameAsync(game.PublisherUserId)).Count >= maxSubmissions)
                return GameResult.MaxSubmissions;

            foreach (var video in game.Videos.ToList())
            {
                game.Videos.Remove(video);
                game.Videos.Add(VideoEmbedExtension.UrlToEmbedCode(video));
            }

            game.Videos ??= new List<string>();
            game.Images ??= new List<string>();

            await _dbContext.Games.AddAsync(game);

            return await _dbContext.SaveChangesAsync() >= 1 ? GameResult.Success : GameResult.Failure;
        }

        /// <summary>
        /// Create/Add a new rating to a game.
        /// </summary>
        public async Task<GameResult> AddRatingAsync(string gameId, string userId, float rating)
        {
            var game = await GetGameAsync(gameId);
            if(game == null) return GameResult.NotFound;

            var gameRating = await GetRatingAsync(gameId, userId);
            if (gameRating != null) return GameResult.AlreadyRated;

            await _dbContext.GameRatings.AddAsync(new GameRating(userId, game.Id, rating));

            await _dbContext.SaveChangesAsync();

            var averageRating = await GetAverageRating(gameId);

            if (averageRating <= 0)
                averageRating = rating;

            game.Rating = averageRating;

            return await UpdateGameAsync(game);
        }

        private async Task<float> GetAverageRating(string gameId)
        {
            var gameRatings = await GetGameRatingsAsync(gameId);

            int ratingNum = gameRatings.Count;
            float ratings = 0;

            if (ratingNum <= 0) return ratings;

            for (int i = 0; i < ratingNum; i++)
            {
                ratings += gameRatings[i].Rating;
            }

            ratings /= ratingNum;

            return ratings;
        }

        public async Task<List<GameRating>> GetGameRatingsAsync(string gameId)
        {
            return (await _dbContext.GameRatings.ToListAsync()).Where(r => r.GameId == gameId).ToList();
        }

        /// <summary>
        /// Get a users rating for a game.
        /// </summary>
        public async Task<GameRating> GetRatingAsync(string gameId, string userId)
        {
            return await _dbContext.GameRatings.FirstOrDefaultAsync(r => r.UserId == userId && r.GameId == gameId);
        }

        /// <summary>
        /// Get a rating for a game.
        /// </summary>
        public async Task<GameRating> GetRatingAsync(int id)
        {
            return await _dbContext.GameRatings.FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Remove a new rating from a game.
        /// </summary>
        public async Task<GameResult> RemoveRatingAsync(string gameId, string userId)
        {
            var game = await GetGameAsync(gameId);
            if (game == null) return GameResult.NotFound;

            var gameRating = await GetRatingAsync(gameId, userId);
            if(gameRating == null) return GameResult.NotFound;

            _dbContext.GameRatings.Remove(gameRating);

            await _dbContext.SaveChangesAsync();

            var averageRating = await GetAverageRating(gameId);

            game.Rating = averageRating;

            if (game.Rating < 0 || !(await GetGameRatingsAsync(game.Id)).Any()) 
                game.Rating = 0;

            return await UpdateGameAsync(game);
        }

        public async Task<GameResult> RemoveRatingAsync(int id)
        {
            var gameRating = await GetRatingAsync(id);
            if (gameRating == null) return GameResult.NotFound;

            var game = await GetGameAsync(gameRating.GameId);
            if (game == null) return GameResult.NotFound;

            _dbContext.GameRatings.Remove(gameRating);

            await _dbContext.SaveChangesAsync();

            var averageRating = await GetAverageRating(game.Id);

            game.Rating = averageRating;

            if (game.Rating < 0 || !(await GetGameRatingsAsync(game.Id)).Any())
                game.Rating = 0;

            return await UpdateGameAsync(game);
        }


        /// <summary>
        /// Updates a preexisting game
        /// </summary>

        public async Task<GameResult> UpdateGameAsync(Game game)
        {
            foreach (var video in game.Videos.ToList())
            {
                game.Videos.Remove(video);
                game.Videos.Add(VideoEmbedExtension.UrlToEmbedCode(video));
            }

            game.Videos ??= new List<string>();
            game.Images ??= new List<string>();

            game.LastEdited = DateTime.Now.ToString();

            _dbContext.Games.Update(game);

            return await _dbContext.SaveChangesAsync() >= 1 ? GameResult.Success : GameResult.Failure;
        }

        /// <summary>
        /// Remove a game that exists using it's id.
        /// </summary>

        public async Task<GameResult> RemoveGameAsync(string gameId)
        {
            var gameToRemove = await GetGameAsync(gameId);

            if(gameToRemove == null) return GameResult.NotFound;

            _dbContext.Games.Remove(gameToRemove);

            return await _dbContext.SaveChangesAsync() >= 1 ? GameResult.Success : GameResult.Failure;
        }

        /// <summary>
        /// Remove a image from a game.
        /// </summary>

        public async Task<GameResult> RemoveImageAsync(string gameId, string image)
        {
            var game = await GetGameAsync(gameId);
            if(game == null) return GameResult.NotFound;

            game.Images.Remove(image);

            return await UpdateGameAsync(game);
        }

        /// <summary>
        /// Remove a video from a game.
        /// </summary>

        public async Task<GameResult> RemoveVideoAsync(string gameId, string video)
        {
            var game = await GetGameAsync(gameId);
            if (game == null) return GameResult.NotFound;

            game.Videos.Remove(video);

            return await UpdateGameAsync(game);
        }

        /// <summary>
        /// Remove a game that exists using it's id and while verifying the userId
        /// </summary>
        public async Task<GameResult> RemoveGameAsync(string gameId, string userId)
        {
            var gameToRemove = await GetGameAsync(gameId);

            if (gameToRemove == null) return GameResult.NotFound;
            if(!gameToRemove.PublisherUserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
                return GameResult.Failure;

            _dbContext.Games.Remove(gameToRemove);

            return await _dbContext.SaveChangesAsync() >= 1 ? GameResult.Success : GameResult.Failure;
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
