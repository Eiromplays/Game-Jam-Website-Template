using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameJam.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameJam.Areas.Identity
{
    public class IdentitySeed
    {
        private readonly IConfiguration _configuration;

        public IdentitySeed(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Create a role if it does not exist.
        /// </summary>
        /// <param name="serviceProvider">Service Provider</param>
        public void CreateRole(IServiceProvider serviceProvider)
        {
            Task.Run(() => CreateRoleAsync(serviceProvider));
        }

        /// <summary>
        /// Create a role if it does not exist (async).
        /// </summary>
        /// <param name="serviceProvider">Service Provider</param>
        public async Task CreateRoleAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            //Resolve RoleManager with DI help
            var roleManager = (RoleManager<IdentityRole>)scope.ServiceProvider.GetService(typeof(RoleManager<IdentityRole>));

            var roleNames =
                (IOptions<DefaultRoleNames>) scope.ServiceProvider.GetService(typeof(IOptions<DefaultRoleNames>));

            if (roleManager == null || roleNames?.Value == null) return;

            var roles = new List<string>{ roleNames.Value.AdministratorRoleName, roleNames.Value.ParticipantRoleName };
            foreach (var role in roles)
            {
                var roleExists = await roleManager.RoleExistsAsync(role).ConfigureAwait(false);

                if (roleExists) continue;
                var result = await roleManager.CreateAsync(new IdentityRole(role)).ConfigureAwait(false);

                if(result.Succeeded)
                    Console.WriteLine($"Successfully added role {role}");

                foreach (var identityError in result.Errors)
                {
                    Console.WriteLine($"Error occurred while creating role {role}");
                    Console.WriteLine($"Error Code {identityError.Code}");
                    Console.WriteLine($"Error Description {identityError.Description}");
                }
            }
        }
    }
}
