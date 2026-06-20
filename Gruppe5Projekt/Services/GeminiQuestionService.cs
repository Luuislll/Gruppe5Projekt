using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using Gruppe5Projekt.Models;
using Type = Google.GenAI.Types.Type;

namespace Gruppe5Projekt.Services;

/// <summary>
/// Kapselt die Anbindung an die Google-Gemini-API über das offizielle SDK.
/// Erzeugt aus dem hochgeladenen Kapitel-Material (PDF) einen
/// Multiple-Choice-Fragenvorschlag und gibt ihn als JSON-String zurück.
/// </summary>
public class GeminiQuestionService
{
    private const string Model = "gemini-2.5-flash";

    private readonly Client _geminiClient;

    public GeminiQuestionService(Client geminiClient)
    {
        _geminiClient = geminiClient;
    }

    /// <summary>
    /// Lädt das PDF zu Gemini hoch und lässt daraus eine MC-Frage generieren.
    /// Über das Response-Schema wird erzwungen, dass die Antwort valides JSON in
    /// der erwarteten Struktur ist (Fragentext + vier Antwortoptionen).
    /// </summary>
    public async Task<GenerierteFrage> GenerateQuestionAsync(
        Kapitel kapitel, Stream pdfStream, CancellationToken cancellationToken = default)
    {
        // PDF in binärer Form (Byte-Array) in den Speicher laden.
        await using var buffer = new MemoryStream();
        await pdfStream.CopyToAsync(buffer, cancellationToken);
        var pdfBytes = buffer.ToArray();

        // Datei vor der eigentlichen Anfrage zu Gemini hochladen.
        var uploadedFile = await _geminiClient.Files.UploadAsync(
            pdfBytes, "kapitel.pdf", new UploadFileConfig { MimeType = "application/pdf" });

        var prompt =
            $"Erstelle aus der beigefügten PDF eine Multiple-Choice-Frage für das Kapitel " +
            $"\"{kapitel.Titel}\". Die Frage (fragentext) muss genau vier Antwortoptionen " +
            $"(antworten) haben, von denen genau eine korrekt ist (istRichtig = true), die " +
            $"übrigen falsch (istRichtig = false). Formuliere Frage und Antworten auf Deutsch.";

        // Anfrage aus Prompt und Datei zusammensetzen.
        Content content = new() { Parts = new List<Part>() };
        content.Parts.Add(new Part { Text = prompt });
        content.Parts.Add(new Part
        {
            FileData = new FileData { FileUri = uploadedFile.Uri, MimeType = "application/pdf" }
        });

        // Schema erzwingt die JSON-Struktur: Fragentext + Liste aus Antwortoptionen.
        Schema questionInfo = new()
        {
            Properties = new Dictionary<string, Schema>
            {
                { "fragentext", new Schema { Type = Type.String, Title = "fragentext" } },
                {
                    "antworten", new Schema
                    {
                        Type = Type.Array,
                        Title = "antworten",
                        Items = new Schema
                        {
                            Type = Type.Object,
                            Properties = new Dictionary<string, Schema>
                            {
                                { "antworttext", new Schema { Type = Type.String, Title = "antworttext" } },
                                { "istRichtig", new Schema { Type = Type.Boolean, Title = "istRichtig" } }
                            },
                            PropertyOrdering = ["antworttext", "istRichtig"],
                            Required = ["antworttext", "istRichtig"]
                        }
                    }
                }
            },
            PropertyOrdering = ["fragentext", "antworten"],
            Required = ["fragentext", "antworten"],
            Title = "questionInfo",
            Type = Type.Object
        };

        GenerateContentConfig config = new()
        {
            ResponseSchema = questionInfo,
            ResponseMimeType = "application/json"
        };

        var response = await _geminiClient.Models.GenerateContentAsync(
            model: Model,
            contents: content,
            config: config);

        var json = response.Candidates?[0].Content?.Parts?[0].Text;
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("Gemini hat keine verwertbare Antwort geliefert.");
        }

        var frage = JsonSerializer.Deserialize<GenerierteFrage>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (frage is null || string.IsNullOrWhiteSpace(frage.Fragentext) || frage.Antworten.Count == 0)
        {
            throw new InvalidOperationException("Gemini hat keine verwertbare Frage geliefert.");
        }

        return frage;
    }

    /// <summary>
    /// Lässt eine bestehende MC-Frage von Gemini überprüfen. Bewertet wird, ob
    /// Frage und Antworten sprachlich korrekt sind und ob genau eine Antwort
    /// inhaltlich richtig ist, die Frage also eindeutig beantwortbar ist.
    /// </summary>
    public async Task<PruefErgebnis> VerifyQuestionAsync(
        string fragentext,
        IReadOnlyList<GenerierteAntwort> antworten,
        CancellationToken cancellationToken = default)
    {
        // Antwortoptionen für den Prompt aufbereiten – inkl. der hinterlegten
        // Markierung (richtig/falsch), damit Gemini diese mitbewerten kann.
        var optionenText = string.Join("\n", antworten.Select((a, i) =>
            $"{i + 1}. {a.Antworttext} [{(a.IstRichtig ? "als richtig markiert" : "als falsch markiert")}]"));

        var prompt =
            "Du bist Prüfer für Multiple-Choice-Fragen. Überprüfe die folgende Frage samt " +
            "Antwortoptionen und bewerte streng:\n" +
            "1. sprachlichKorrekt: Sind Frage und alle Antworten sprachlich einwandfrei " +
            "(Rechtschreibung, Grammatik, klare Formulierung)?\n" +
            "2. genauEineRichtigeAntwort: Ist inhaltlich genau eine Antwort korrekt – nicht " +
            "mehrere und nicht keine – und ist genau diese auch als richtig markiert?\n" +
            "3. beantwortbar: Lässt sich die Frage dadurch eindeutig beantworten?\n" +
            "Gib eine kurze Begründung auf Deutsch und – falls nötig – konkrete " +
            "Verbesserungsvorschläge.\n\n" +
            $"Frage: {fragentext}\n\nAntwortoptionen:\n{optionenText}";

        Content content = new() { Parts = new List<Part> { new() { Text = prompt } } };

        // Schema erzwingt eine strukturierte Bewertung als JSON.
        Schema ergebnisSchema = new()
        {
            Properties = new Dictionary<string, Schema>
            {
                { "sprachlichKorrekt", new Schema { Type = Type.Boolean, Title = "sprachlichKorrekt" } },
                { "genauEineRichtigeAntwort", new Schema { Type = Type.Boolean, Title = "genauEineRichtigeAntwort" } },
                { "beantwortbar", new Schema { Type = Type.Boolean, Title = "beantwortbar" } },
                { "begruendung", new Schema { Type = Type.String, Title = "begruendung" } },
                {
                    "verbesserungsvorschlaege", new Schema
                    {
                        Type = Type.Array,
                        Title = "verbesserungsvorschlaege",
                        Items = new Schema { Type = Type.String }
                    }
                }
            },
            PropertyOrdering =
                ["sprachlichKorrekt", "genauEineRichtigeAntwort", "beantwortbar", "begruendung", "verbesserungsvorschlaege"],
            Required = ["sprachlichKorrekt", "genauEineRichtigeAntwort", "beantwortbar", "begruendung"],
            Title = "pruefErgebnis",
            Type = Type.Object
        };

        GenerateContentConfig config = new()
        {
            ResponseSchema = ergebnisSchema,
            ResponseMimeType = "application/json"
        };

        var response = await _geminiClient.Models.GenerateContentAsync(
            model: Model,
            contents: content,
            config: config);

        var json = response.Candidates?[0].Content?.Parts?[0].Text;
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("Gemini hat keine verwertbare Antwort geliefert.");
        }

        var ergebnis = JsonSerializer.Deserialize<PruefErgebnis>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (ergebnis is null)
        {
            throw new InvalidOperationException("Gemini hat kein verwertbares Prüfergebnis geliefert.");
        }

        return ergebnis;
    }
}

/// <summary>Von Gemini generierte Frage samt Antwortoptionen.</summary>
public record GenerierteFrage(string Fragentext, List<GenerierteAntwort> Antworten);

/// <summary>Eine generierte Antwortoption.</summary>
public record GenerierteAntwort(string Antworttext, bool IstRichtig);

/// <summary>
/// Ergebnis der KI-gestützten Prüfung einer MC-Frage. Die Frage gilt nur dann
/// als gültig, wenn sie sprachlich korrekt, eindeutig beantwortbar ist und genau
/// eine richtige Antwort besitzt.
/// </summary>
public record PruefErgebnis(
    bool SprachlichKorrekt,
    bool GenauEineRichtigeAntwort,
    bool Beantwortbar,
    string Begruendung,
    List<string>? Verbesserungsvorschlaege = null)
{
    /// <summary>True, wenn alle Prüfkriterien erfüllt sind.</summary>
    public bool IstGueltig => SprachlichKorrekt && GenauEineRichtigeAntwort && Beantwortbar;
}
