using System;
using System.Collections.Generic;

namespace GameJam.Api.Models
{
    /// <summary>
    /// The default implementation of <see cref="Game{TKey}"/> which uses a string as a primary key.
    /// </summary>
    public sealed class Game : Game<string>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Game"/>.
        /// </summary>
        /// <remarks>
        /// The Id property is initialized to form a new GUID string value.
        /// </remarks>
        public Game()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Game"/>.
        /// </summary>
        /// <param name="name">The games name.</param>
        /// <param name="publisherUserId">The games publisher</param>
        /// <param name="downloadLink">The games download link/location</param>
        /// <param name="images">The games showcase images</param>
        /// <param name="videos">The games showcase videos</param>
        /// <param name="approved">Determines if the game will show up or not.</param>
        /// <remarks>
        /// The Id property is initialized to form a new GUID string value.
        /// </remarks>
        public Game(string name, string description, string publisherUserId, string downloadLink, List<string> images, List<string> videos,
            bool approved) : this()
        {
            Name = name;
            Description = description;
            PublisherUserId = publisherUserId;
            DownloadLink = downloadLink;
            Images = images;
            Videos = videos;
            Approved = approved;
        }
    }

    /// <summary>
    /// Represents a game
    /// </summary>
    /// <typeparam name="TKey">The type used for the primary key for the game.</typeparam>
    public class Game<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Game{TKey}"/>.
        /// </summary>
        public Game() { }

        /// <summary>
        /// Initializes a new instance of <see cref="Game{TKey}"/>.
        /// </summary>
        /// <param name="name">The game's name.</param>
        public Game(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the primary key for this game.
        /// </summary>
        public virtual TKey Id { get; set; }

        /// <summary>
        /// Gets or sets the name for this game.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the description for this game.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the publisher for this game.
        /// </summary>
        public string PublisherUserId { get; set; }

        /// <summary>
        /// Gets or sets the download link for this game.
        /// </summary>
        public string DownloadLink { get; set; }

        /// <summary>
        /// Gets or sets the images for this game.
        /// </summary>
        public List<string> Images { get; set; }

        /// <summary>
        /// Gets or sets the videos for this game.
        /// </summary>
        public List<string> Videos { get; set; }

        /// <summary>
        /// Gets or sets the approved state for this game.
        /// </summary>
        public bool Approved { get; set; }

        /// <summary>
        /// Returns the name for this game.
        /// </summary>
        public override string ToString()
            => Name;
    }
}
