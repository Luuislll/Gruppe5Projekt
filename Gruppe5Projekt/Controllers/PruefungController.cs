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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pruefung == null)
            {
                return NotFound();
            }

            return View(pruefung);
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
