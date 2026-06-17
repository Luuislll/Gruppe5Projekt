namespace Gruppe5Projekt.Models;

/// <summary>
/// Verbindungstabelle (n:m) zwischen Pruefung und MCFrage.
/// Zusammengesetzter Primärschlüssel aus beiden Fremdschlüsseln.
/// </summary>
public class PruefungMCFrage
{
    public int PruefungId { get; set; }
    public Pruefung? Pruefung { get; set; }

    public int MCFrageId { get; set; }
    public MCFrage? MCFrage { get; set; }
}
