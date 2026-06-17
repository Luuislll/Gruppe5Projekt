using System.ComponentModel.DataAnnotations;

namespace Gruppe5Projekt.Models;

/// <summary>
/// Eine Antwortoption einer MC-Frage.
/// </summary>
public class MCAntwortOption
{
    public int Id { get; set; }

    [Required]
    public string Antworttext { get; set; } = string.Empty;

    public bool IstRichtig { get; set; }

    // FK → MCFrage (genau eine Frage)
    public int MCFrageId { get; set; }
    public MCFrage? MCFrage { get; set; }
}
