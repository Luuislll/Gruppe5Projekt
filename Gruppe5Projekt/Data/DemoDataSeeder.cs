using Gruppe5Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace Gruppe5Projekt.Data;

/// <summary>
/// Legt Demodaten an bzw. entfernt sie wieder. Das Seeding ist idempotent
/// (legt nur an, wenn der Bestand leer ist); das Entfernen löscht alle
/// Lehrveranstaltungen, wobei das Cascade-Delete Kapitel, Fragen, Antworten
/// und Prüfungen mit aufräumt.
/// </summary>
public static class DemoDataSeeder
{
    // Feste Seeds sorgen für reproduzierbare, aber abwechslungsreiche Daten.
    private const int RandomSeed = 5;

    private static readonly string[] Kursthemen =
    [
        "Einführung in die Programmierung", "Datenbanksysteme", "Algorithmen und Datenstrukturen",
        "Software Engineering", "Betriebssysteme", "Rechnernetze", "Theoretische Informatik",
        "Webentwicklung", "Künstliche Intelligenz", "Maschinelles Lernen", "IT-Sicherheit",
        "Verteilte Systeme", "Computergrafik", "Mobile Anwendungen", "Cloud Computing",
        "Datenanalyse", "Mensch-Computer-Interaktion", "Compilerbau", "Mikrocontroller-Programmierung",
        "Diskrete Mathematik", "Lineare Algebra", "Statistik für Informatiker", "Quantencomputing",
        "Robotik", "Bildverarbeitung"
    ];

    private static readonly string[] Dozenten =
    [
        "Prof. Dr. Anna Schmidt", "Prof. Dr. Thomas Müller", "Dr. Julia Weber", "Prof. Dr. Michael Fischer",
        "Dr. Sarah Wagner", "Prof. Dr. Stefan Becker", "Dr. Laura Hoffmann", "Prof. Dr. Markus Schulz",
        "Dr. Christina Koch", "Prof. Dr. Andreas Richter", "Dr. Nina Bauer", "Prof. Dr. Peter Klein"
    ];

    private static readonly string[] Kapiteltitel =
    [
        "Grundlagen", "Vertiefung", "Fortgeschrittene Konzepte", "Praktische Anwendungen",
        "Theoretischer Hintergrund", "Fallstudien", "Best Practices", "Ausblick und Trends"
    ];

    /// <summary>
    /// Legt Demodaten an, falls noch keine vorhanden sind (idempotent).
    /// </summary>
    public static async Task SeedAsync(AppDbContext db)
    {
        // Idempotenz: nur seeden, wenn noch keine Lehrveranstaltungen existieren.
        if (await db.Lehrveranstaltungen.AnyAsync())
        {
            return;
        }

        var rng = new Random(RandomSeed);
        var lehrveranstaltungen = new List<Lehrveranstaltung>();

        for (var i = 0; i < 20; i++)
        {
            var titel = Kursthemen[i % Kursthemen.Length];
            var lv = new Lehrveranstaltung
            {
                Titel = titel,
                Dozentenname = Dozenten[rng.Next(Dozenten.Length)],
                Niveau = rng.Next(2) == 0 ? Niveau.Bachelor : Niveau.Master
            };

            // 2–3 Kapitel pro Lehrveranstaltung
            var anzahlKapitel = rng.Next(2, 4);
            var alleFragenDerLv = new List<MCFrage>();

            for (var k = 0; k < anzahlKapitel; k++)
            {
                var kapitel = new Kapitel
                {
                    Titel = $"{Kapiteltitel[k % Kapiteltitel.Length]} – {titel}",
                    Kapitelnummer = k + 1
                };

                // 2–3 Fragen pro Kapitel
                var anzahlFragen = rng.Next(2, 4);
                for (var f = 0; f < anzahlFragen; f++)
                {
                    var frage = ErzeugeFrage(titel, k + 1, f + 1, rng);
                    kapitel.MCFragen.Add(frage);
                    alleFragenDerLv.Add(frage);
                }

                lv.Kapitel.Add(kapitel);
            }

            // 1 Prüfung pro Lehrveranstaltung (übersichtlich halten)
            var anzahlPruefungen = 1;
            for (var p = 0; p < anzahlPruefungen; p++)
            {
                var pruefung = new Pruefung
                {
                    // Termine rund um das aktuelle Datum verteilen
                    Datum = DateTime.Today.AddDays(rng.Next(-120, 121)).AddHours(9)
                };

                // Zufällige Teilmenge der Fragen dieser LV der Prüfung zuordnen
                var fragenFuerPruefung = alleFragenDerLv
                    .OrderBy(_ => rng.Next())
                    .Take(Math.Min(alleFragenDerLv.Count, rng.Next(2, 5)))
                    .ToList();

                foreach (var frage in fragenFuerPruefung)
                {
                    pruefung.PruefungMCFragen.Add(new PruefungMCFrage { MCFrage = frage });
                }

                lv.Pruefungen.Add(pruefung);
            }

            lehrveranstaltungen.Add(lv);
        }

        db.Lehrveranstaltungen.AddRange(lehrveranstaltungen);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Entfernt sämtliche Demodaten. Durch das Cascade-Delete werden Kapitel,
    /// Fragen, Antwortoptionen, Prüfungen und n:m-Verknüpfungen automatisch
    /// mit gelöscht. Gibt die Anzahl entfernter Lehrveranstaltungen zurück.
    /// </summary>
    public static async Task<int> RemoveDemoDataAsync(AppDbContext db)
    {
        var demoLvs = await db.Lehrveranstaltungen.ToListAsync();

        if (demoLvs.Count == 0)
        {
            return 0;
        }

        db.Lehrveranstaltungen.RemoveRange(demoLvs);
        await db.SaveChangesAsync();
        return demoLvs.Count;
    }

    private static MCFrage ErzeugeFrage(string thema, int kapitelNr, int frageNr, Random rng)
    {
        var frage = new MCFrage
        {
            Fragentext = $"{thema} – Kapitel {kapitelNr}, Frage {frageNr}: Welche Aussage ist korrekt?"
        };

        // 4 Antwortoptionen, eine davon (zufällig platziert) ist richtig
        var richtigeOption = rng.Next(4);
        for (var a = 0; a < 4; a++)
        {
            var istRichtig = a == richtigeOption;
            frage.AntwortOptionen.Add(new MCAntwortOption
            {
                Antworttext = istRichtig
                    ? $"Antwort {(char)('A' + a)} (korrekt)"
                    : $"Antwort {(char)('A' + a)}",
                IstRichtig = istRichtig
            });
        }

        return frage;
    }
}
