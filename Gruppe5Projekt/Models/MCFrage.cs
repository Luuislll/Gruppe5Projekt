using System.ComponentModel.DataAnnotations;

namespace Gruppe5Projekt.Models;

/// <summary>
/// Eine Multiple-Choice-Frage, die zu genau einem Kapitel gehört.
/// </summary>
public class MCFrage
{
    public int Id { get; set; }

    [Required]
    public string Fragentext { get; set; } = string.Empty;

    // FK → Kapitel (genau ein Kapitel)
    public int KapitelId { get; set; }
    public Kapitel? Kapitel { get; set; }

    // Navigation: 1:n
    public ICollection<MCAntwortOption> AntwortOptionen { get; set; } = new List<MCAntwortOption>();

    // Navigation: n:m zu Pruefung über PruefungMCFrage
    public ICollection<PruefungMCFrage> PruefungMCFragen { get; set; } = new List<PruefungMCFrage>();
}
