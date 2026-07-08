using Microsoft.AspNetCore.Identity;
using WebApi.Models;

namespace WebApi.Data.Seeders;

public class ApplicationUserSeeder : ISeeder
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationUserSeeder(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        if (_userManager.Users.Any()) return;

        List<(ApplicationUser user, string password)> users = new()
        {
            (new ApplicationUser
            {
                FirstName = "Admin",
                LastName = "User",
                UserName = "admin",
                Email = "admin@example.com",
                CreatedAt = DateTime.UtcNow
            }, "password"),

            (new ApplicationUser
            {
                FirstName = "Regular",
                LastName = "User",
                UserName = "user",
                Email = "user@example.com",
                CreatedAt = DateTime.UtcNow
            }, "password")
        };

        foreach ((ApplicationUser user, string password) in users)
        {
            await _userManager.CreateAsync(user, password);
        }
    }
}