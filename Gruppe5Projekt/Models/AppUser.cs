using Microsoft.AspNetCore.Identity;
namespace Gruppe5Projekt.Models;

public class AppUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
    