using GameJam.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameJam.Areas.Identity.Data
{
    public class GameJamDbContext : IdentityDbContext<GameJamUser>
    {
        public DbSet<Game> Games { get; set; }
        public GameJamDbContext(DbContextOptions<GameJamDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
