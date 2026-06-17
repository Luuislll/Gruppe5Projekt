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
    public class MCAntwortOptionController : Controller
    {
        private readonly AppDbContext _context;

        public MCAntwortOptionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MCAntwortOption
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.MCAntwortOptionen.Include(m => m.MCFrage);
            return View(await appDbContext.ToListAsync());
        }

        // GET: MCAntwortOption/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mCAntwortOption = await _context.MCAntwortOptionen
                .Include(m => m.MCFrage)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mCAntwortOption == null)
            {
                return NotFound();
            }

            return View(mCAntwortOption);
        }

        // GET: MCAntwortOption/Create
        public IActionResult Create()
        {
            ViewData["MCFrageId"] = new SelectList(_context.MCFragen, "Id", "Fragentext");
            return View();
        }

        // POST: MCAntwortOption/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Antworttext,IstRichtig,MCFrageId")] MCAntwortOption mCAntwortOption)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mCAntwortOption);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MCFrageId"] = new SelectList(_context.MCFragen, "Id", "Fragentext", mCAntwortOption.MCFrageId);
            return View(mCAntwortOption);
        }

        // GET: MCAntwortOption/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mCAntwortOption = await _context.MCAntwortOptionen.FindAsync(id);
            if (mCAntwortOption == null)
            {
                return NotFound();
            }
            ViewData["MCFrageId"] = new SelectList(_context.MCFragen, "Id", "Fragentext", mCAntwortOption.MCFrageId);
            return View(mCAntwortOption);
        }

        // POST: MCAntwortOption/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Antworttext,IstRichtig,MCFrageId")] MCAntwortOption mCAntwortOption)
        {
            if (id != mCAntwortOption.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mCAntwortOption);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MCAntwortOptionExists(mCAntwortOption.Id))
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
            ViewData["MCFrageId"] = new SelectList(_context.MCFragen, "Id", "Fragentext", mCAntwortOption.MCFrageId);
            return View(mCAntwortOption);
        }

        // GET: MCAntwortOption/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mCAntwortOption = await _context.MCAntwortOptionen
                .Include(m => m.MCFrage)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mCAntwortOption == null)
            {
                return NotFound();
            }

            return View(mCAntwortOption);
        }

        // POST: MCAntwortOption/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mCAntwortOption = await _context.MCAntwortOptionen.FindAsync(id);
            if (mCAntwortOption != null)
            {
                _context.MCAntwortOptionen.Remove(mCAntwortOption);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MCAntwortOptionExists(int id)
        {
            return _context.MCAntwortOptionen.Any(e => e.Id == id);
        }
    }
}
