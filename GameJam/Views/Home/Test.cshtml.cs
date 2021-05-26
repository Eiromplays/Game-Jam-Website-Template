using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace GameJam.Views.Home
{
    public class TestModel : PageModel
    {
        public async Task OnGetAsync()
        {
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Type {claim.Type} Value {claim.Value}");
            }
        }
    }
}
