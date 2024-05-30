using Microsoft.AspNetCore.Identity;

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Admin", "user" };

        IdentityResult roleResult;

        foreach (var roleName in roleNames)
        {
            // Create the roles and seed them to the database
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                // Create the roles and seed them to the database
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        IdentityUser user = await userManager.FindByEmailAsync("admin@hrs.com");

        if (user == null)
        {
            user = new IdentityUser()
            {
                UserName = "admin@hrs.com",
                Email = "admin@hrs.com",
            };
            await userManager.CreateAsync(user, "Admin");
        }
        await userManager.AddToRoleAsync(user, "Admin");
    }
}
