using Gruppe5Projekt.Data;
using Gruppe5Projekt.Models;
using Gruppe5Projekt.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gruppe5Projekt.Controllers
{
    /// <summary>
    /// Verwaltet den Fragenkatalog eines Kapitels: das Anlegen, Bearbeiten und
    /// Löschen von MC-Fragen samt ihren vier Antwortmöglichkeiten.
    /// </summary>
    public class FragenkatalogController : Controller
    {
        private readonly AppDbContext _context;

        public FragenkatalogController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Fragenkatalog?kapitelId=5
        // Zeigt alle Fragen des Kapitels mit ihren Antwortoptionen.
        public async Task<IActionResult> Index(int kapitelId)
        {
            var kapitel = await _context.Kapitel
                .Include(k => k.MCFragen)
                .ThenInclude(f => f.AntwortOptionen)
                .FirstOrDefaultAsync(k => k.Id == kapitelId);

            if (kapitel == null)
            {
                return NotFound();
            }

            return View(kapitel);
        }

        // GET: Fragenkatalog/Create?kapitelId=5
        public async Task<IActionResult> Create(int kapitelId)
        {
            var kapitel = await _context.Kapitel.FindAsync(kapitelId);
            if (kapitel == null)
            {
                return NotFound();
            }

            var viewModel = new FrageEingabeViewModel
            {
                KapitelId = kapitel.Id,
                KapitelTitel = kapitel.Titel
            };
            return View(viewModel);
        }

        // POST: Fragenkatalog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FrageEingabeViewModel viewModel)
        {
            var kapitel = await _context.Kapitel.FindAsync(viewModel.KapitelId);
            if (kapitel == null)
            {
                return NotFound();
            }

            ValidateAntworten(viewModel);

            if (!ModelState.IsValid)
            {
                viewModel.KapitelTitel = kapitel.Titel;
                return View(viewModel);
            }

            var frage = new MCFrage
            {
                KapitelId = viewModel.KapitelId,
                Fragentext = viewModel.Fragentext,
                AntwortOptionen = ErzeugeAntwortOptionen(viewModel)
            };

            _context.MCFragen.Add(frage);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { kapitelId = viewModel.KapitelId });
        }

        // GET: Fragenkatalog/Edit/10
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var frage = await _context.MCFragen
                .Include(f => f.Kapitel)
                .Include(f => f.AntwortOptionen)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (frage == null)
            {
                return NotFound();
            }

            var optionen = frage.AntwortOptionen.OrderBy(o => o.Id).ToList();
            var viewModel = new FrageEingabeViewModel
            {
                Id = frage.Id,
                KapitelId = frage.KapitelId,
                KapitelTitel = frage.Kapitel?.Titel,
                Fragentext = frage.Fragentext,
                Antworten = optionen.Select(o => o.Antworttext).ToList(),
                KorrekteAntwortIndex = optionen.FindIndex(o => o.IstRichtig)
            };

            // Auf genau vier Felder auffüllen, falls Altdaten abweichen.
            while (viewModel.Antworten.Count < FrageEingabeViewModel.AnzahlAntworten)
            {
                viewModel.Antworten.Add(string.Empty);
            }
            if (viewModel.KorrekteAntwortIndex < 0)
            {
                viewModel.KorrekteAntwortIndex = 0;
            }

            return View(viewModel);
        }

        // POST: Fragenkatalog/Edit/10
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FrageEingabeViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            var frage = await _context.MCFragen
                .Include(f => f.Kapitel)
                .Include(f => f.AntwortOptionen)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (frage == null)
            {
                return NotFound();
            }

            ValidateAntworten(viewModel);

            if (!ModelState.IsValid)
            {
                viewModel.KapitelTitel = frage.Kapitel?.Titel;
                return View(viewModel);
            }

            frage.Fragentext = viewModel.Fragentext;

            // Alte Antwortoptionen ersetzen (einfacher als ein Abgleich pro Option).
            _context.MCAntwortOptionen.RemoveRange(frage.AntwortOptionen);
            frage.AntwortOptionen = ErzeugeAntwortOptionen(viewModel);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { kapitelId = frage.KapitelId });
        }

        // GET: Fragenkatalog/Delete/10
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var frage = await _context.MCFragen
                .Include(f => f.Kapitel)
                .Include(f => f.AntwortOptionen)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (frage == null)
            {
                return NotFound();
            }

            return View(frage);
        }

        // POST: Fragenkatalog/Delete/10
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var frage = await _context.MCFragen.FindAsync(id);
            if (frage == null)
            {
                return NotFound();
            }

            var kapitelId = frage.KapitelId;
            _context.MCFragen.Remove(frage); // AntwortOptionen werden per Cascade gelöscht
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { kapitelId });
        }

        /// <summary>
        /// Prüft, dass alle vier Antworttexte ausgefüllt sind. Den Pflicht-Check
        /// für Fragentext und die Markierung der korrekten Antwort übernehmen die
        /// Data-Annotations am ViewModel.
        /// </summary>
        private void ValidateAntworten(FrageEingabeViewModel viewModel)
        {
            for (var i = 0; i < FrageEingabeViewModel.AnzahlAntworten; i++)
            {
                var text = i < viewModel.Antworten.Count ? viewModel.Antworten[i] : null;
                if (string.IsNullOrWhiteSpace(text))
                {
                    ModelState.AddModelError($"Antworten[{i}]", $"Bitte Antwort {i + 1} ausfüllen.");
                }
            }
        }

        private static List<MCAntwortOption> ErzeugeAntwortOptionen(FrageEingabeViewModel viewModel)
        {
            return Enumerable.Range(0, FrageEingabeViewModel.AnzahlAntworten)
                .Select(i => new MCAntwortOption
                {
                    Antworttext = viewModel.Antworten[i],
                    IstRichtig = i == viewModel.KorrekteAntwortIndex
                })
                .ToList();
        }
    }
}
