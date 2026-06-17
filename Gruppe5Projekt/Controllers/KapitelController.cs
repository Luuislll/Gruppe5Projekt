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
    public class KapitelController : Controller
    {
        private readonly AppDbContext _context;

        public KapitelController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Kapitel
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Kapitel.Include(k => k.Lehrveranstaltung);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Kapitel/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kapitel = await _context.Kapitel
                .Include(k => k.Lehrveranstaltung)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (kapitel == null)
            {
                return NotFound();
            }

            return View(kapitel);
        }

        // GET: Kapitel/Create
        public IActionResult Create()
        {
            ViewData["LehrveranstaltungId"] = new SelectList(_context.Lehrveranstaltungen, "Id", "Dozentenname");
            return View();
        }

        // POST: Kapitel/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titel,Kapitelnummer,Vorlesungsfolien,LehrveranstaltungId")] Kapitel kapitel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kapitel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LehrveranstaltungId"] = new SelectList(_context.Lehrveranstaltungen, "Id", "Dozentenname", kapitel.LehrveranstaltungId);
            return View(kapitel);
        }

        // GET: Kapitel/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kapitel = await _context.Kapitel.FindAsync(id);
            if (kapitel == null)
            {
                return NotFound();
            }
            ViewData["LehrveranstaltungId"] = new SelectList(_context.Lehrveranstaltungen, "Id", "Dozentenname", kapitel.LehrveranstaltungId);
            return View(kapitel);
        }

        // POST: Kapitel/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titel,Kapitelnummer,Vorlesungsfolien,LehrveranstaltungId")] Kapitel kapitel)
        {
            if (id != kapitel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kapitel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KapitelExists(kapitel.Id))
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
            ViewData["LehrveranstaltungId"] = new SelectList(_context.Lehrveranstaltungen, "Id", "Dozentenname", kapitel.LehrveranstaltungId);
            return View(kapitel);
        }

        // GET: Kapitel/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kapitel = await _context.Kapitel
                .Include(k => k.Lehrveranstaltung)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (kapitel == null)
            {
                return NotFound();
            }

            return View(kapitel);
        }

        // POST: Kapitel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kapitel = await _context.Kapitel.FindAsync(id);
            if (kapitel != null)
            {
                _context.Kapitel.Remove(kapitel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KapitelExists(int id)
        {
            return _context.Kapitel.Any(e => e.Id == id);
        }
    }
}
