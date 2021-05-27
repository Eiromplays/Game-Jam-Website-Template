using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(GameJam.Areas.Identity.IdentityHostingStartup))]
namespace GameJam.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<GameJamDbContext>( options =>
                {
                    options.UseNpgsql(
                        context.Configuration.GetConnectionString("GameJamDbContextConnection"));

                });

                services.AddDefaultIdentity<GameJamUser>(options =>
                    {
                        options.SignIn.RequireConfirmedAccount = true;
                    })
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<GameJamDbContext>();
            });
        }
    }
}