using Microsoft.AspNetCore.Identity;

namespace LoginPart.Identity
{
    public class SeedRoles
    {
        public static async Task Initialize(IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager)
        {
            var roleNames = new[] { "MainAdmin", "Admin", "Referee" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
