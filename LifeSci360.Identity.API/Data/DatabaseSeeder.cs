using LifeSci360.Identity.API.Models;
using LifeSci360.Shared.Enums;
using Microsoft.AspNetCore.Identity;

public class DatabaseSeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<User> _userManager;

    public DatabaseSeeder(
        RoleManager<ApplicationRole> roleManager,
        UserManager<User> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedAdminAsync();
    }

    private async Task SeedRolesAsync()
    {
        // pulls directly from your UserRole enum
        var roles = Enum.GetValues(typeof(UserRole))
                        .Cast<UserRole>()
                        .Select(r => r.ToString());

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    Description = $"{role} role for LifeSci360",
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                });
            }
        }
    }

    private async Task SeedAdminAsync()
    {
        var adminEmail = "admin@lifesci360.com";

        if (await _userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Admin",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager
                             .CreateAsync(admin, "Admin@12345!");

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(
                    admin,
                    UserRole.Admin.ToString()  // uses enum directly
                );
            }
        }
    }
}