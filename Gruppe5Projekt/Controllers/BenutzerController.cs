using Gruppe5Projekt.Data;
using Gruppe5Projekt.Models;
using Gruppe5Projekt.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Gruppe5Projekt.Controllers;

/// <summary>
/// Benutzerverwaltung: Übersicht aller Benutzer, Anlegen neuer Benutzer mit
/// Rollenzuweisung sowie Löschen (mit Sicherheitsabfrage). Nur für Administratoren.
/// </summary>
[Authorize(Roles = IdentitySeeder.AdminRole)]
public class BenutzerController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public BenutzerController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: /Benutzer
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var liste = new List<BenutzerListeViewModel>();

        foreach (var user in users)
        {
            liste.Add(new BenutzerListeViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Rollen = await _userManager.GetRolesAsync(user)
            });
        }

        return View(liste);
    }

    // GET: /Benutzer/Create
    public async Task<IActionResult> Create()
    {
        return View(new BenutzerErstellenViewModel { VerfuegbareRollen = await LadeRollenAsync() });
    }

    // POST: /Benutzer/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BenutzerErstellenViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (await _roleManager.RoleExistsAsync(model.Rolle))
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Rolle);
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                ModelState.AddModelError(nameof(model.Rolle), "Die gewählte Rolle existiert nicht.");
            }
        }

        model.VerfuegbareRollen = await LadeRollenAsync();
        return View(model);
    }

    // GET: /Benutzer/Delete/{id} – Sicherheitsabfrage vor dem Löschen
    public async Task<IActionResult> Delete(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(new BenutzerListeViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Rollen = await _userManager.GetRolesAsync(user)
        });
    }

    // POST: /Benutzer/Delete/{id}
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return RedirectToAction(nameof(Index));
        }

        // Verhindern, dass man sich selbst löscht.
        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Fehler"] = "Sie können Ihren eigenen Account nicht löschen.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.DeleteAsync(user);
        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> LadeRollenAsync()
    {
        return await _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
            .ToListAsync();
    }
}
