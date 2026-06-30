using Gruppe5Projekt.Models;
using Microsoft.AspNetCore.Identity;

namespace Gruppe5Projekt.Data;

/// <summary>
/// Legt die für die Anwendung benötigten Identity-Rollen an und stellt einen
/// Standard-Administrator bereit. Das Seeding ist idempotent: bereits
/// vorhandene Rollen bzw. der Admin-Benutzer werden nicht erneut angelegt.
/// </summary>
public static class IdentitySeeder
{
    /// <summary>Rolle mit allen Verwaltungsrechten.</summary>
    public const string AdminRole = "Admin";

    /// <summary>Rolle für normale, angemeldete Benutzer.</summary>
    public const string UserRole = "User";

    // Zugangsdaten des Standard-Admins (nur für die Entwicklung gedacht).
    private const string AdminEmail = "admin@klausuren.local";
    private const string AdminPassword = "Admin123!";

    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
    {
        // Beide Rollen bereitstellen, falls noch nicht vorhanden.
        foreach (var role in new[] { AdminRole, UserRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Standard-Admin anlegen, damit ein Login möglich ist.
        if (await userManager.FindByEmailAsync(AdminEmail) is null)
        {
            var admin = new AppUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator"
            };

            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, AdminRole);
            }
        }
    }
}
