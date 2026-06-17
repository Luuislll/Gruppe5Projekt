using System.ComponentModel.DataAnnotations;

namespace Gruppe5Projekt.Models;

/// <summary>
/// Ein Kapitel einer Lehrveranstaltung. Enthält MC-Fragen.
/// </summary>
public class Kapitel
{
    public int Id { get; set; }

    [Required]
    public string Titel { get; set; } = string.Empty;

    [Required]
    public int Kapitelnummer { get; set; }

    /// <summary>
    /// Dateipfad der hochgeladenen Vorlesungsfolien (PDF). Optional.
    /// </summary>
    public string? Vorlesungsfolien { get; set; }

    // FK → Lehrveranstaltung (genau eine LV)
    public int LehrveranstaltungId { get; set; }
    public Lehrveranstaltung? Lehrveranstaltung { get; set; }

    // Navigation: 1:n
    public ICollection<MCFrage> MCFragen { get; set; } = new List<MCFrage>();
}
