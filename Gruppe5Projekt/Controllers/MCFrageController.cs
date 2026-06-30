using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gruppe5Projekt.Data;
using Gruppe5Projekt.Models;

namespace Gruppe5Projekt.Controllers
{
    // Schreibzugriffe erfordern eine Anmeldung; Lese-Actions sind für Gäste frei.
    [Authorize]
    public class MCFrageController : Controller
    {
        private readonly AppDbContext _context;

        public MCFrageController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MCFrage
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.MCFragen.Include(m => m.Kapitel);
            return View(await appDbContext.ToListAsync());
        }

        // GET: MCFrage/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mCFrage = await _context.MCFragen
                .Include(m => m.Kapitel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mCFrage == null)
            {
                return NotFound();
            }

            return View(mCFrage);
        }

        // GET: MCFrage/Create
        public IActionResult Create()
        {
            ViewData["KapitelId"] = new SelectList(_context.Kapitel, "Id", "Titel");
            return View();
        }

        // POST: MCFrage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Fragentext,KapitelId")] MCFrage mCFrage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mCFrage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["KapitelId"] = new SelectList(_context.Kapitel, "Id", "Titel", mCFrage.KapitelId);
            return View(mCFrage);
        }

        // GET: MCFrage/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mCFrage = await _context.MCFragen.FindAsync(id);
            if (mCFrage == null)
            {
                return NotFound();
            }
            ViewData["KapitelId"] = new SelectList(_context.Kapitel, "Id", "Titel", mCFrage.KapitelId);
            return View(mCFrage);
        }

        // POST: MCFrage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Fragentext,KapitelId")] MCFrage mCFrage)
        {
            if (id != mCFrage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mCFrage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MCFrageExists(mCFrage.Id))
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
            ViewData["KapitelId"] = new SelectList(_context.Kapitel, "Id", "Titel", mCFrage.KapitelId);
            return View(mCFrage);
        }

        // GET: MCFrage/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mCFrage = await _context.MCFragen
                .Include(m => m.Kapitel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mCFrage == null)
            {
                return NotFound();
            }

            return View(mCFrage);
        }

        // POST: MCFrage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mCFrage = await _context.MCFragen.FindAsync(id);
            if (mCFrage != null)
            {
                _context.MCFragen.Remove(mCFrage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MCFrageExists(int id)
        {
            return _context.MCFragen.Any(e => e.Id == id);
        }
    }
}
