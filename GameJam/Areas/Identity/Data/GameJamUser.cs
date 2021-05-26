using Microsoft.AspNetCore.Identity;

namespace GameJam.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the GameJamUser class
    public class GameJamUser : IdentityUser
    {
        public string ProfilePicture { get; set; }
    }
}
