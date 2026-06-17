using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gruppe5Projekt.Data;
using Gruppe5Projekt.Models;

namespace Gruppe5Projekt.Controllers
{
    public class PruefungMCFrageController : Controller
    {
        private readonly AppDbContext _context;

        public PruefungMCFrageController(AppDbContext context)
        {
            _context = context;
        }

        // GET: PruefungMCFrage
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.PruefungMCFragen.Include(p => p.MCFrage).Include(p => p.Pruefung);
            return View(await appDbContext.ToListAsync());
        }

        // GET: PruefungMCFrage/Details?pruefungId=1&mcFrageId=2
        public async Task<IActionResult> Details(int? pruefungId, int? mcFrageId)
        {
            if (pruefungId == null || mcFrageId == null)
            {
                return NotFound();
            }

            var pruefungMCFrage = await _context.PruefungMCFragen
                .Include(p => p.MCFrage)
                .Include(p => p.Pruefung)
                .FirstOrDefaultAsync(m => m.PruefungId == pruefungId && m.MCFrageId == mcFrageId);
            if (pruefungMCFrage == null)
            {
                return NotFound();
            }

            return View(pruefungMCFrage);
        }

        // GET: PruefungMCFrage/Create
        public IActionResult Create()
        {
            ViewData["MCFrageId"] = new SelectList(_context.MCFragen, "Id", "Fragentext");
            ViewData["PruefungId"] = new SelectList(_context.Pruefungen, "Id", "Id");
            return View();
        }

        // POST: PruefungMCFrage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PruefungId,MCFrageId")] PruefungMCFrage pruefungMCFrage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pruefungMCFrage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MCFrageId"] = new SelectList(_context.MCFragen, "Id", "Fragentext", pruefungMCFrage.MCFrageId);
            ViewData["PruefungId"] = new SelectList(_context.Pruefungen, "Id", "Id", pruefungMCFrage.PruefungId);
            return View(pruefungMCFrage);
        }

        // GET: PruefungMCFrage/Edit?pruefungId=1&mcFrageId=2
        public async Task<IActionResult> Edit(int? pruefungId, int? mcFrageId)
        {
            if (pruefungId == null || mcFrageId == null)
            {
                return NotFound();
            }

            var pruefungMCFrage = await _context.PruefungMCFragen.FindAsync(pruefungId, mcFrageId);
            if (pruefungMCFrage == null)
            {
                return NotFound();
            }
            ViewData["MCFrageId"] = new SelectList(_context.MCFragen, "Id", "Fragentext", pruefungMCFrage.MCFrageId);
            ViewData["PruefungId"] = new SelectList(_context.Pruefungen, "Id", "Id", pruefungMCFrage.PruefungId);
            return View(pruefungMCFrage);
        }

        // POST: PruefungMCFrage/Edit?pruefungId=1&mcFrageId=2
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int pruefungId, int mcFrageId, [Bind("PruefungId,MCFrageId")] PruefungMCFrage pruefungMCFrage)
        {
            if (pruefungId != pruefungMCFrage.PruefungId || mcFrageId != pruefungMCFrage.MCFrageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pruefungMCFrage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PruefungMCFrageExists(pruefungMCFrage.PruefungId, pruefungMCFrage.MCFrageId))
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
            ViewData["MCFrageId"] = new SelectList(_context.MCFragen, "Id", "Fragentext", pruefungMCFrage.MCFrageId);
            ViewData["PruefungId"] = new SelectList(_context.Pruefungen, "Id", "Id", pruefungMCFrage.PruefungId);
            return View(pruefungMCFrage);
        }

        // GET: PruefungMCFrage/Delete?pruefungId=1&mcFrageId=2
        public async Task<IActionResult> Delete(int? pruefungId, int? mcFrageId)
        {
            if (pruefungId == null || mcFrageId == null)
            {
                return NotFound();
            }

            var pruefungMCFrage = await _context.PruefungMCFragen
                .Include(p => p.MCFrage)
                .Include(p => p.Pruefung)
                .FirstOrDefaultAsync(m => m.PruefungId == pruefungId && m.MCFrageId == mcFrageId);
            if (pruefungMCFrage == null)
            {
                return NotFound();
            }

            return View(pruefungMCFrage);
        }

        // POST: PruefungMCFrage/Delete?pruefungId=1&mcFrageId=2
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int pruefungId, int mcFrageId)
        {
            var pruefungMCFrage = await _context.PruefungMCFragen.FindAsync(pruefungId, mcFrageId);
            if (pruefungMCFrage != null)
            {
                _context.PruefungMCFragen.Remove(pruefungMCFrage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PruefungMCFrageExists(int pruefungId, int mcFrageId)
        {
            return _context.PruefungMCFragen.Any(e => e.PruefungId == pruefungId && e.MCFrageId == mcFrageId);
        }
    }
}
