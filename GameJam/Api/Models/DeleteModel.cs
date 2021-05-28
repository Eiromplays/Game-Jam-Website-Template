using System.ComponentModel.DataAnnotations;

namespace GameJam.Api.Models
{
    public class DeleteModel
    {
        [Required]
        public string Value { get; set; }

        [Required]
        public string GameId { get; set; }
    }
}
