using GameJam.Api.Interfaces;
using GameJam.Api.Models;
using GameJam.Api.Services;
using GameJam.Areas.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;

namespace GameJam
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings:noreply"));

            services.Configure<DefaultRoleNames>(Configuration.GetSection("DefaultRoles"));

            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddScoped<IGameRepository, GameRepository>();

            ConfigureExternalProviders(services.AddAuthentication());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            Task.Run(() => new IdentitySeed(Configuration).CreateRole(app.ApplicationServices));
        }

        // Custom methods to register the external providers used

        public void ConfigureExternalProviders(AuthenticationBuilder authenticationBuilder)
        {
            ConfigureGoogle(authenticationBuilder);
            ConfigureDiscord(authenticationBuilder);
            ConfigureFeide(authenticationBuilder);
        }

        public void ConfigureGoogle(AuthenticationBuilder authenticationBuilder)
        {
            IConfigurationSection googleAuthSection =
                Configuration.GetSection("Authentication:Google");

            if (!googleAuthSection.GetValue<bool>("Enabled")) return;

            authenticationBuilder.AddGoogle(options =>
            {
                options.ClientId = googleAuthSection["ClientId"];
                options.ClientSecret = googleAuthSection["ClientSecret"];
            });
        }

        public void ConfigureDiscord(AuthenticationBuilder authenticationBuilder)
        {
            IConfigurationSection discordAuthSection =
                Configuration.GetSection("Authentication:Discord");

            if (!discordAuthSection.GetValue<bool>("Enabled")) return;

            authenticationBuilder.AddDiscord(options =>
            {
                options.Scope.Add("email");
                options.ClientId = discordAuthSection["ClientId"];
                options.ClientSecret = discordAuthSection["ClientSecret"];
            });
        }

        public void ConfigureFeide(AuthenticationBuilder authenticationBuilder)
        {
            IConfigurationSection feideAuthSection =
                Configuration.GetSection("Authentication:Feide");

            if (!feideAuthSection.GetValue<bool>("Enabled")) return;

            authenticationBuilder.AddOAuth("Feide", options =>
            {
                options.ClientId = feideAuthSection["ClientId"];
                options.ClientSecret = feideAuthSection["ClientSecret"];
                options.CallbackPath = new PathString("/signin-oauth");

                options.AuthorizationEndpoint = "https://auth.dataporten.no/oauth/authorization";
                options.TokenEndpoint = "https://auth.dataporten.no/oauth/token";

                options.SaveTokens = true;
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "userid");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "userinfo-name");
                options.ClaimActions.MapJsonKey("feide:email", "email");
                options.ClaimActions.MapJsonKey("feide:photo", "userinfo-photo");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var request =
                            new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", context.AccessToken);
                        var response = await context.Backchannel.SendAsync(request,
                            HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();
                        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                        context.RunClaimActions(json.RootElement);
                    }
                };
            });
        }
    }
}
