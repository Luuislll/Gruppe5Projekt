using System.ComponentModel.DataAnnotations;

namespace Gruppe5Projekt.Models;

/// <summary>
/// Eine Lehrveranstaltung (LV) mit Kapiteln und Prüfungen.
/// </summary>
public class Lehrveranstaltung
{
    public int Id { get; set; }

    [Required]
    public string Titel { get; set; } = string.Empty;

    [Required]
    public string Dozentenname { get; set; } = string.Empty;

    [Required]
    public Niveau Niveau { get; set; }

    /// <summary>
    /// Kennzeichnet automatisch erzeugte Demodaten. Über dieses Flag können
    /// die Demodaten später gefahrlos wieder entfernt werden (Cascade-Delete
    /// räumt Kapitel, Fragen, Antworten und Prüfungen mit auf).
    /// </summary>
    public bool IstDemo { get; set; }

    // Navigation: 1:n
    public ICollection<Kapitel> Kapitel { get; set; } = new List<Kapitel>();
    public ICollection<Pruefung> Pruefungen { get; set; } = new List<Pruefung>();
}
