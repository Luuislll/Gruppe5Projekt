using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gruppe5Projekt.Data;
using Gruppe5Projekt.Models;
using Gruppe5Projekt.Services;

namespace Gruppe5Projekt.Controllers
{
    public class KapitelController : Controller
    {
        private readonly AppDbContext _context;
        private readonly KapitelMaterialService _materialService;

        public KapitelController(AppDbContext context, KapitelMaterialService materialService)
        {
            _context = context;
            _materialService = materialService;
        }

        // GET: Kapitel
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Kapitel.Include(k => k.Lehrveranstaltung).OrderBy(k => k.Kapitelnummer);
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
        public async Task<IActionResult> Create([Bind("Id,Titel,Kapitelnummer,LehrveranstaltungId")] Kapitel kapitel, IFormFile? pdfDatei)
        {
            if (pdfDatei is not null && !IstPdf(pdfDatei))
            {
                ModelState.AddModelError(nameof(pdfDatei), "Es sind nur PDF-Dateien erlaubt.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(kapitel);
                await _context.SaveChangesAsync();

                // Datei erst nach dem Speichern hochladen, damit die Kapitel-Id feststeht.
                if (pdfDatei is not null)
                {
                    kapitel.Vorlesungsfolien = await _materialService.UploadAsync(kapitel.Id, pdfDatei);
                    await _context.SaveChangesAsync();
                }

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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titel,Kapitelnummer,LehrveranstaltungId")] Kapitel kapitel, IFormFile? pdfDatei)
        {
            if (id != kapitel.Id)
            {
                return NotFound();
            }

            if (pdfDatei is not null && !IstPdf(pdfDatei))
            {
                ModelState.AddModelError(nameof(pdfDatei), "Es sind nur PDF-Dateien erlaubt.");
            }

            if (ModelState.IsValid)
            {
                var existing = await _context.Kapitel.FindAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Titel = kapitel.Titel;
                existing.Kapitelnummer = kapitel.Kapitelnummer;
                existing.LehrveranstaltungId = kapitel.LehrveranstaltungId;

                if (pdfDatei is not null)
                {
                    var neuerBlob = await _materialService.UploadAsync(existing.Id, pdfDatei);

                    // Alte Datei entfernen, falls sie einen anderen Namen hatte.
                    if (!string.IsNullOrEmpty(existing.Vorlesungsfolien) && existing.Vorlesungsfolien != neuerBlob)
                    {
                        await _materialService.DeleteAsync(existing.Vorlesungsfolien);
                    }

                    existing.Vorlesungsfolien = neuerBlob;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LehrveranstaltungId"] = new SelectList(_context.Lehrveranstaltungen, "Id", "Dozentenname", kapitel.LehrveranstaltungId);
            return View(kapitel);
        }

        // GET: Kapitel/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var kapitel = await _context.Kapitel.FindAsync(id);
            if (kapitel == null || string.IsNullOrEmpty(kapitel.Vorlesungsfolien))
            {
                return NotFound();
            }

            var datei = await _materialService.DownloadAsync(kapitel.Vorlesungsfolien);
            if (datei == null)
            {
                return NotFound();
            }

            return File(datei.Value.Content, datei.Value.ContentType, datei.Value.FileName);
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
                await _materialService.DeleteAsync(kapitel.Vorlesungsfolien);
                _context.Kapitel.Remove(kapitel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KapitelExists(int id)
        {
            return _context.Kapitel.Any(e => e.Id == id);
        }

        private static bool IstPdf(IFormFile datei)
        {
            return datei.ContentType == "application/pdf"
                   || Path.GetExtension(datei.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        }
    }
}
