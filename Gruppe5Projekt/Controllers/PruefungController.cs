using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gruppe5Projekt.Data;
using Gruppe5Projekt.Models;

namespace Gruppe5Projekt.Controllers
{
    public class PruefungController : Controller
    {
        private readonly AppDbContext _context;

        public PruefungController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Pruefung
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Pruefungen.Include(p => p.Lehrveranstaltung);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Pruefung/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pruefung = await _context.Pruefungen
                .Include(p => p.Lehrveranstaltung)
                .Include(p => p.PruefungMCFragen)
                    .ThenInclude(pf => pf.MCFrage)
                        .ThenInclude(f => f!.Kapitel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pruefung == null)
            {
                return NotFound();
            }

            return View(pruefung);
        }

        // POST: Pruefung/FragenZusammenstellen/5
        // Stellt für die Prüfung zufällig und ohne Duplikate kapitelübergreifend MC-Fragen
        // aus allen Kapiteln der zugehörigen Lehrveranstaltung zusammen. Die gewünschte
        // Anzahl wird per Pop-up übergeben. Eine bestehende Zusammenstellung wird ersetzt.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FragenZusammenstellen(int id, int anzahl)
        {
            var pruefung = await _context.Pruefungen
                .Include(p => p.PruefungMCFragen)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pruefung == null)
            {
                return NotFound();
            }

            if (anzahl < 1)
            {
                TempData["FragenFehler"] = "Bitte eine Anzahl größer als 0 angeben.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Alle Fragen-Ids aller Kapitel dieser Lehrveranstaltung sammeln.
            var verfuegbareFragenIds = await _context.MCFragen
                .Where(f => f.Kapitel!.LehrveranstaltungId == pruefung.LehrveranstaltungId)
                .Select(f => f.Id)
                .ToListAsync();

            if (verfuegbareFragenIds.Count == 0)
            {
                TempData["FragenFehler"] =
                    "Für diese Lehrveranstaltung sind in keinem Kapitel Fragen vorhanden.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Zufällig und ohne Duplikate auswählen – höchstens so viele wie vorhanden.
            var anzahlEffektiv = Math.Min(anzahl, verfuegbareFragenIds.Count);
            var ausgewaehlteIds = verfuegbareFragenIds
                .OrderBy(_ => Guid.NewGuid())
                .Take(anzahlEffektiv)
                .ToList();

            // Bestehende Zusammenstellung verwerfen und neu zusammenstellen.
            _context.PruefungMCFragen.RemoveRange(pruefung.PruefungMCFragen);
            foreach (var frageId in ausgewaehlteIds)
            {
                _context.PruefungMCFragen.Add(new PruefungMCFrage
                {
                    PruefungId = pruefung.Id,
                    MCFrageId = frageId
                });
            }
            await _context.SaveChangesAsync();

            if (anzahlEffektiv < anzahl)
            {
                TempData["FragenHinweis"] =
                    $"Es wurden {anzahlEffektiv} von {anzahl} gewünschten Fragen ausgewählt, " +
                    "da nicht mehr Fragen verfügbar sind.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Pruefung/Create
        public IActionResult Create()
        {
            ViewData["LehrveranstaltungId"] = new SelectList(_context.Lehrveranstaltungen, "Id", "Dozentenname");
            return View();
        }

        // POST: Pruefung/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Datum,LehrveranstaltungId")] Pruefung pruefung)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pruefung);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LehrveranstaltungId"] = new SelectList(_context.Lehrveranstaltungen, "Id", "Dozentenname", pruefung.LehrveranstaltungId);
            return View(pruefung);
        }

        // GET: Pruefung/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pruefung = await _context.Pruefungen.FindAsync(id);
            if (pruefung == null)
            {
                return NotFound();
            }
            ViewData["LehrveranstaltungId"] = new SelectList(_context.Lehrveranstaltungen, "Id", "Dozentenname", pruefung.LehrveranstaltungId);
            return View(pruefung);
        }

        // POST: Pruefung/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Datum,LehrveranstaltungId")] Pruefung pruefung)
        {
            if (id != pruefung.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pruefung);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PruefungExists(pruefung.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["LehrveranstaltungId"] = new SelectList(_context.Lehrveranstaltungen, "Id", "Dozentenname", pruefung.LehrveranstaltungId);
            return View(pruefung);
        }

        // GET: Pruefung/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pruefung = await _context.Pruefungen
                .Include(p => p.Lehrveranstaltung)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pruefung == null)
            {
                return NotFound();
            }

            return View(pruefung);
        }

        // POST: Pruefung/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pruefung = await _context.Pruefungen.FindAsync(id);
            if (pruefung != null)
            {
                _context.Pruefungen.Remove(pruefung);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PruefungExists(int id)
        {
            return _context.Pruefungen.Any(e => e.Id == id);
        }
    }
}
