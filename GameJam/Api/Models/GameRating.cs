using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameJam.Api.Models
{
    public class GameRating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string UserId { get; set; }
        public string GameId { get; set; }

        public float Rating { get; set; }

        public GameRating(string userId, string gameId, float rating)
        {
            UserId = userId;
            GameId = gameId;
            Rating = rating;
        }
    }
}
