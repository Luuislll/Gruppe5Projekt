using System.ComponentModel.DataAnnotations;

namespace Gruppe5Projekt.Models.ViewModels;

/// <summary>
/// Eingabemodell zum Anlegen bzw. Bearbeiten einer MC-Frage im Fragenkatalog
/// eines Kapitels. Eine Frage besteht aus einem Fragentext und genau vier
/// Antwortmöglichkeiten, von denen genau eine als korrekt markiert ist.
/// </summary>
public class FrageEingabeViewModel
{
    /// <summary>Id der MC-Frage. 0 beim Neuanlegen.</summary>
    public int Id { get; set; }

    /// <summary>Kapitel, zu dessen Fragenkatalog die Frage gehört.</summary>
    public int KapitelId { get; set; }

    /// <summary>Titel des Kapitels (nur zur Anzeige).</summary>
    public string? KapitelTitel { get; set; }

    [Required(ErrorMessage = "Bitte einen Fragentext eingeben.")]
    [Display(Name = "Fragentext")]
    public string Fragentext { get; set; } = string.Empty;

    /// <summary>
    /// Die vier Antworttexte. Index entspricht <see cref="KorrekteAntwortIndex"/>.
    /// </summary>
    public List<string> Antworten { get; set; } = new() { string.Empty, string.Empty, string.Empty, string.Empty };

    /// <summary>Index (0–3) der korrekten Antwort innerhalb von <see cref="Antworten"/>.</summary>
    [Range(0, 3, ErrorMessage = "Bitte die korrekte Antwort markieren.")]
    [Display(Name = "Korrekte Antwort")]
    public int KorrekteAntwortIndex { get; set; }

    /// <summary>Anzahl der Antwortmöglichkeiten pro Frage.</summary>
    public const int AnzahlAntworten = 4;
}
