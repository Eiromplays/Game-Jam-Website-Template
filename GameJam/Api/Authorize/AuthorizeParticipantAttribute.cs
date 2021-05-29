using System.Linq;
using GameJam.Api.Models;
using GameJam.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using GameJam.Api.Interfaces;

namespace GameJam.Api.Authorize
{
    public class AuthorizeParticipantAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            Task.Run(async () => await CheckIfAdminAsync(context));
        }

        // Check if the user is part of the participant role.
        // If the user is not part of that role it will redirect them to the Access Denied page.
        private async Task CheckIfAdminAsync(AuthorizationFilterContext context)
        {
            using var scope = context.HttpContext.RequestServices.CreateScope();
            var services = scope.ServiceProvider;

            var defaultRoleNames = services.GetService<IOptions<DefaultRoleNames>>()?.Value;

            var userManager = services.GetService<UserManager<GameJamUser>>();

            var gameRepository = services.GetService<IGameRepository>();

            if (userManager == null || gameRepository == null)
            {
                context.HttpContext.Response.Redirect("/Home/AccessDenied");
                return;
            }

            var user = await userManager.GetUserAsync(context.HttpContext.User);
            var isParticipant = await userManager.IsInRoleAsync(user, defaultRoleNames?.ParticipantRoleName);
            if (!isParticipant && !(await gameRepository.GetUsersGameAsync(user.Id)).Any())
            {
                context.HttpContext.Response.Redirect("/Home/AccessDenied");
            }
        }
    }
}