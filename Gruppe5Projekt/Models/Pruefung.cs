using System.ComponentModel.DataAnnotations;

namespace Gruppe5Projekt.Models;

/// <summary>
/// Eine Prüfung (Klausur) zu einer Lehrveranstaltung.
/// </summary>
public class Pruefung
{
    public int Id { get; set; }

    [Required]
    public DateTime Datum { get; set; }

    // FK → Lehrveranstaltung (genau eine LV)
    public int LehrveranstaltungId { get; set; }
    public Lehrveranstaltung? Lehrveranstaltung { get; set; }

    // Navigation: n:m zu MCFrage über PruefungMCFrage
    public ICollection<PruefungMCFrage> PruefungMCFragen { get; set; } = new List<PruefungMCFrage>();
}
