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
    public class LehrveranstaltungenController : Controller
    {
        private readonly AppDbContext _context;

        public LehrveranstaltungenController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Lehrveranstaltungen
        public async Task<IActionResult> Index()
        {
            return View(await _context.Lehrveranstaltungen.ToListAsync());
        }

        // GET: Lehrveranstaltungen/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lehrveranstaltung = await _context.Lehrveranstaltungen
                .Include(l => l.Kapitel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lehrveranstaltung == null)
            {
                return NotFound();
            }

            return View(lehrveranstaltung);
        }

        // GET: Lehrveranstaltungen/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Lehrveranstaltungen/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titel,Dozentenname,Niveau")] Lehrveranstaltung lehrveranstaltung)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lehrveranstaltung);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(lehrveranstaltung);
        }

        // GET: Lehrveranstaltungen/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lehrveranstaltung = await _context.Lehrveranstaltungen.FindAsync(id);
            if (lehrveranstaltung == null)
            {
                return NotFound();
            }
            return View(lehrveranstaltung);
        }

        // POST: Lehrveranstaltungen/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titel,Dozentenname,Niveau")] Lehrveranstaltung lehrveranstaltung)
        {
            if (id != lehrveranstaltung.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lehrveranstaltung);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LehrveranstaltungExists(lehrveranstaltung.Id))
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
            return View(lehrveranstaltung);
        }

        // GET: Lehrveranstaltungen/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lehrveranstaltung = await _context.Lehrveranstaltungen
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lehrveranstaltung == null)
            {
                return NotFound();
            }

            return View(lehrveranstaltung);
        }

        // POST: Lehrveranstaltungen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lehrveranstaltung = await _context.Lehrveranstaltungen.FindAsync(id);
            if (lehrveranstaltung != null)
            {
                _context.Lehrveranstaltungen.Remove(lehrveranstaltung);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LehrveranstaltungExists(int id)
        {
            return _context.Lehrveranstaltungen.Any(e => e.Id == id);
        }
    }
}
