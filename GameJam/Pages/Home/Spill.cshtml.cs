using GameJam.Api.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameJam.Pages.Home
{
    public class SpillModel : PageModel
    {
        public readonly List<Game> Games = new List<Game>
        {
            new Game
            {
                Approved = true,
                Name = "Truckmaster 2",
                Picture = "https://imgur.com/0BxPhj7.png",
                Publisher = "Eirik Sjøløkken"
            },
            new Game
            {
                Approved = true,
                Name = "Truckmaster 2",
                Picture = "https://imgur.com/0BxPhj7.png",
                Publisher = "Eirik Sjøløkken"
            },
            new Game
            {
                Approved = true,
                Name = "Truckmaster 2",
                Picture = "https://imgur.com/0BxPhj7.png",
                Publisher = "Eirik Sjøløkken"
            }
        };

        public async Task OnGetAsync()
        {
            
        }
    }
}
