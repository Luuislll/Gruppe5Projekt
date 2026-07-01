using Gruppe5Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace Gruppe5Projekt.Data;

/// <summary>
/// Legt Demodaten an bzw. entfernt sie wieder. Das Seeding ist idempotent
/// (legt nur an, wenn der Bestand leer ist); das Entfernen löscht alle
/// Lehrveranstaltungen, wobei das Cascade-Delete Kapitel, Fragen, Antworten
/// und Prüfungen mit aufräumt.
///
/// Die Kapitel und Fragen stammen aus einem kuratierten Fach-Katalog
/// (<see cref="Kurskatalog"/>), damit die Demodaten praxisnah wirken.
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

    // Fallback-Kapiteltitel, falls ein Kursthema (noch) nicht im Katalog steht.
    private static readonly string[] Kapiteltitel =
    [
        "Grundlagen", "Vertiefung", "Fortgeschrittene Konzepte", "Praktische Anwendungen",
        "Theoretischer Hintergrund", "Fallstudien", "Best Practices", "Ausblick und Trends"
    ];

    /// <summary>Eine MC-Frage mit genau einer richtigen und drei falschen Antworten.</summary>
    private sealed record FrageVorlage(string Frage, string Richtig, string[] Falsch);

    /// <summary>Ein Kapitel mit seinen Fragen.</summary>
    private sealed record KapitelVorlage(string Titel, FrageVorlage[] Fragen);

    // Kuratierter Fach-Katalog: pro Kursthema praxisnahe Kapitel und Fragen.
    private static readonly Dictionary<string, KapitelVorlage[]> Kurskatalog = new()
    {
        ["Einführung in die Programmierung"] =
        [
            new("Variablen und Datentypen",
            [
                new("Welcher Datentyp eignet sich in C# am besten zum Speichern eines Wahrheitswerts?",
                    "bool", ["int", "string", "double"]),
                new("Was beschreibt der Begriff \"Variable\" in der Programmierung?",
                    "Ein benannter Speicherbereich für einen Wert",
                    ["Eine unveränderliche Konstante", "Ein Schlüsselwort der Sprache", "Ein Kommentar im Quellcode"]),
                new("Welche Aussage über den Datentyp int ist korrekt?",
                    "Er speichert ganze Zahlen",
                    ["Er speichert nur positive Zahlen", "Er speichert Text", "Er speichert ausschließlich Fließkommazahlen"]),
            ]),
            new("Kontrollstrukturen",
            [
                new("Wozu dient eine while-Schleife?",
                    "Sie wiederholt einen Block, solange eine Bedingung wahr ist",
                    ["Sie führt Code genau einmal aus", "Sie definiert eine Funktion", "Sie deklariert eine Variable"]),
                new("Was bewirkt das Schlüsselwort break in einer Schleife?",
                    "Es beendet die Schleife vorzeitig",
                    ["Es überspringt nur den aktuellen Durchlauf", "Es startet die Schleife neu", "Es pausiert das Programm"]),
            ]),
            new("Funktionen und Methoden",
            [
                new("Was gibt eine Methode mit dem Rückgabetyp void zurück?",
                    "Nichts", ["Immer 0", "Einen leeren String", "null"]),
                new("Was versteht man unter einem Parameter einer Methode?",
                    "Einen Eingabewert, den die Methode beim Aufruf erhält",
                    ["Den Rückgabewert der Methode", "Den Namen der Methode", "Eine globale Variable"]),
            ]),
        ],
        ["Datenbanksysteme"] =
        [
            new("Relationales Modell",
            [
                new("Was ist ein Primärschlüssel?",
                    "Ein Attribut, das jeden Datensatz eindeutig identifiziert",
                    ["Ein beliebiges Textfeld", "Ein Verweis auf eine andere Tabelle", "Ein Index zur Beschleunigung"]),
                new("Wofür dient ein Fremdschlüssel?",
                    "Er verweist auf den Primärschlüssel einer anderen Tabelle",
                    ["Er verschlüsselt die Daten", "Er sortiert die Tabelle", "Er entfernt Duplikate"]),
            ]),
            new("SQL-Grundlagen",
            [
                new("Mit welchem SQL-Befehl liest man Daten aus einer Tabelle?",
                    "SELECT", ["INSERT", "UPDATE", "DELETE"]),
                new("Was bewirkt die WHERE-Klausel in einer Abfrage?",
                    "Sie filtert Zeilen nach einer Bedingung",
                    ["Sie sortiert das Ergebnis", "Sie gruppiert Zeilen", "Sie verbindet zwei Tabellen"]),
                new("Welcher Befehl fügt einen neuen Datensatz ein?",
                    "INSERT INTO", ["ADD ROW", "CREATE TABLE", "SELECT"]),
            ]),
            new("Normalisierung",
            [
                new("Welches Ziel verfolgt die Normalisierung?",
                    "Redundanz und Anomalien vermeiden",
                    ["Abfragen beschleunigen", "Tabellen verschlüsseln", "Speicherplatz maximal ausnutzen"]),
                new("Was verlangt die 1. Normalform?",
                    "Dass alle Attribute atomar sind",
                    ["Dass es keinen Primärschlüssel gibt", "Dass jede Tabelle einen Fremdschlüssel hat", "Dass alle Werte numerisch sind"]),
            ]),
        ],
        ["Algorithmen und Datenstrukturen"] =
        [
            new("Komplexität und O-Notation",
            [
                new("Was beschreibt die O-Notation?",
                    "Das asymptotische Wachstum des Aufwands",
                    ["Den exakten Speicherverbrauch in Byte", "Die Laufzeit in Sekunden", "Die Anzahl der Codezeilen"]),
                new("Welche Komplexität hat die binäre Suche in einer sortierten Liste?",
                    "O(log n)", ["O(n)", "O(n²)", "O(1)"]),
                new("Welche Laufzeit hat der Zugriff auf ein Array-Element per Index?",
                    "O(1)", ["O(n)", "O(log n)", "O(n²)"]),
            ]),
            new("Sortierverfahren",
            [
                new("Welche durchschnittliche Laufzeit hat Quicksort?",
                    "O(n log n)", ["O(n)", "O(n²)", "O(log n)"]),
                new("Welches Sortierverfahren hat im schlechtesten Fall eine Laufzeit von O(n²)?",
                    "Bubblesort", ["Mergesort", "Heapsort", "Keines der genannten"]),
            ]),
            new("Grundlegende Datenstrukturen",
            [
                new("Nach welchem Prinzip arbeitet ein Stack?",
                    "LIFO – Last In, First Out",
                    ["FIFO – First In, First Out", "Nach Priorität", "In zufälliger Reihenfolge"]),
                new("Nach welchem Prinzip arbeitet eine Queue?",
                    "FIFO – First In, First Out",
                    ["LIFO – Last In, First Out", "Nach Priorität", "In zufälliger Reihenfolge"]),
            ]),
        ],
        ["Software Engineering"] =
        [
            new("Vorgehensmodelle",
            [
                new("Was kennzeichnet das Wasserfallmodell?",
                    "Sequenziell nacheinander durchlaufene Phasen",
                    ["Kurze Iterationen mit ständigem Feedback", "Den vollständigen Verzicht auf Dokumentation", "Paralleles Arbeiten ohne Planung"]),
                new("Was ist ein zentrales Merkmal agiler Methoden wie Scrum?",
                    "Iterative Entwicklung in kurzen Sprints",
                    ["Ein einziger großer Release am Projektende", "Feste Anforderungen ohne jede Änderung", "Verzicht auf Teamkommunikation"]),
            ]),
            new("Anforderungsanalyse",
            [
                new("Was ist eine funktionale Anforderung?",
                    "Eine konkrete Funktion, die das System leisten soll",
                    ["Eine Aussage über die Performance", "Eine rechtliche Rahmenvorgabe", "Das Budget des Projekts"]),
                new("Was beschreibt eine User Story?",
                    "Eine Anforderung aus Sicht des Nutzers",
                    ["Den Quellcode einer Funktion", "Einen automatisierten Testfall", "Ein Datenbankschema"]),
            ]),
            new("Testen",
            [
                new("Was prüft ein Unit-Test?",
                    "Eine einzelne, isolierte Komponente",
                    ["Das gesamte System im Zusammenspiel", "Die Benutzeroberfläche rein manuell", "Die Netzwerkverbindung"]),
                new("Was versteht man unter einem Integrationstest?",
                    "Das Zusammenspiel mehrerer Komponenten",
                    ["Eine einzelne, isolierte Methode", "Die Einhaltung der Codeformatierung", "Die Rechtschreibung der Dokumentation"]),
            ]),
        ],
        ["Betriebssysteme"] =
        [
            new("Prozesse und Threads",
            [
                new("Was ist ein Prozess?",
                    "Ein Programm in Ausführung mit eigenem Adressraum",
                    ["Eine Quelldatei", "Ein Hardwarebaustein", "Ein Netzwerkprotokoll"]),
                new("Was gilt für Threads desselben Prozesses?",
                    "Sie teilen sich den Adressraum",
                    ["Jeder hat einen eigenen Adressraum", "Sie laufen zwingend auf verschiedenen Rechnern", "Sie haben keinerlei gemeinsame Ressourcen"]),
            ]),
            new("Speicherverwaltung",
            [
                new("Wozu dient virtueller Speicher?",
                    "Er stellt mehr Adressraum bereit, als physisch vorhanden ist",
                    ["Er beschleunigt die CPU-Taktung", "Er verschlüsselt die Daten", "Er ersetzt die Festplatte vollständig"]),
                new("Was versteht man unter Paging?",
                    "Die Aufteilung des Speichers in gleich große Seiten",
                    ["Das Beenden von Prozessen", "Die Priorisierung von Threads", "Das Komprimieren von Dateien"]),
            ]),
            new("Scheduling",
            [
                new("Was ist die Aufgabe des Schedulers?",
                    "Die Zuteilung der CPU an Prozesse",
                    ["Die Verwaltung der Festplatte", "Das Rendern der Oberfläche", "Die Netzwerkkommunikation"]),
                new("Was kennzeichnet präemptives Scheduling?",
                    "Der Scheduler kann einen laufenden Prozess unterbrechen",
                    ["Prozesse laufen stets bis zum Ende", "Es existiert nur ein einziger Prozess", "Die Reihenfolge ist rein zufällig"]),
            ]),
        ],
        ["Rechnernetze"] =
        [
            new("OSI-Modell",
            [
                new("Wie viele Schichten hat das OSI-Referenzmodell?",
                    "7", ["4", "5", "3"]),
                new("Auf welcher OSI-Schicht arbeitet ein Router hauptsächlich?",
                    "Vermittlungsschicht (Schicht 3)",
                    ["Bitübertragungsschicht (Schicht 1)", "Sicherungsschicht (Schicht 2)", "Anwendungsschicht (Schicht 7)"]),
            ]),
            new("TCP/IP",
            [
                new("Welche Eigenschaft hat das TCP-Protokoll?",
                    "Verbindungsorientiert und zuverlässig",
                    ["Verbindungslos und unzuverlässig", "Ausschließlich für Videostreaming", "Nur innerhalb eines LAN nutzbar"]),
                new("Wofür wird UDP typischerweise eingesetzt?",
                    "Für schnelle, verlusttolerante Übertragungen wie Streaming",
                    ["Für garantiert zuverlässige Dateiübertragung", "Für die Stromversorgung", "Für die Verschlüsselung von Mails"]),
            ]),
            new("Adressierung",
            [
                new("Aus wie vielen Bit besteht eine IPv4-Adresse?",
                    "32 Bit", ["64 Bit", "128 Bit", "16 Bit"]),
                new("Wozu dient DNS?",
                    "Zur Auflösung von Domainnamen in IP-Adressen",
                    ["Zur Verschlüsselung von E-Mails", "Zur Vergabe von MAC-Adressen", "Zur Komprimierung von Daten"]),
            ]),
        ],
        ["Theoretische Informatik"] =
        [
            new("Endliche Automaten",
            [
                new("Welche Sprachklasse erkennt ein endlicher Automat?",
                    "Reguläre Sprachen",
                    ["Kontextfreie Sprachen", "Alle entscheidbaren Sprachen", "Nur endliche Mengen"]),
                new("Woraus besteht ein deterministischer endlicher Automat?",
                    "Aus Zuständen, Übergängen, Start- und Endzuständen",
                    ["Nur aus einem Band", "Aus Variablen und Schleifen", "Aus Klassen und Objekten"]),
            ]),
            new("Formale Sprachen",
            [
                new("Welche Sprachklasse steht in der Chomsky-Hierarchie auf Stufe Typ 0?",
                    "Rekursiv aufzählbare Sprachen",
                    ["Reguläre Sprachen", "Kontextfreie Sprachen", "Endliche Sprachen"]),
                new("Womit werden kontextfreie Sprachen erzeugt?",
                    "Mit kontextfreien Grammatiken",
                    ["Mit regulären Ausdrücken", "Mit endlichen Automaten", "Mit Wahrheitstafeln"]),
            ]),
            new("Berechenbarkeit",
            [
                new("Was besagt das Halteproblem?",
                    "Es ist unentscheidbar, ob ein Programm für eine Eingabe hält",
                    ["Jedes Programm hält irgendwann", "Kein Programm hält jemals", "Das Halten hängt nur von der Sprache ab"]),
                new("Welches Modell gilt als Grundlage der Berechenbarkeit?",
                    "Die Turingmaschine",
                    ["Der endliche Automat", "Das relationale Modell", "Das OSI-Modell"]),
            ]),
        ],
        ["Webentwicklung"] =
        [
            new("HTML und CSS",
            [
                new("Wofür ist HTML zuständig?",
                    "Für die Struktur und die Inhalte einer Webseite",
                    ["Für die Datenbankabfragen", "Für das Styling der Seite", "Für die Serverlogik"]),
                new("Wofür wird CSS verwendet?",
                    "Für Layout und Gestaltung",
                    ["Für die Seitenstruktur", "Für die Datenspeicherung", "Für die Verschlüsselung"]),
            ]),
            new("HTTP",
            [
                new("Welche HTTP-Methode wird typischerweise zum Abrufen von Daten genutzt?",
                    "GET", ["POST", "DELETE", "PUT"]),
                new("Was bedeutet der HTTP-Statuscode 404?",
                    "Ressource nicht gefunden",
                    ["Anfrage erfolgreich", "Interner Serverfehler", "Dauerhafte Weiterleitung"]),
                new("Welcher HTTP-Statuscode signalisiert Erfolg?",
                    "200", ["301", "404", "500"]),
            ]),
            new("JavaScript",
            [
                new("Wo wird clientseitiges JavaScript klassischerweise ausgeführt?",
                    "Im Browser des Clients",
                    ["Ausschließlich auf dem Datenbankserver", "Im Betriebssystemkern", "In der Netzwerkkarte"]),
                new("Was ist das DOM?",
                    "Eine Objektdarstellung der HTML-Struktur",
                    ["Ein Datenbanksystem", "Ein CSS-Framework", "Ein Netzwerkprotokoll"]),
            ]),
        ],
        ["Künstliche Intelligenz"] =
        [
            new("Suchverfahren",
            [
                new("Was kennzeichnet eine informierte Suche?",
                    "Sie nutzt eine Heuristik zur Bewertung",
                    ["Sie läuft rein zufällig ab", "Sie hat kein definiertes Ziel", "Sie ignoriert jede Bewertung"]),
                new("Wofür steht der A*-Algorithmus?",
                    "Ein heuristisches Verfahren zur Pfadsuche",
                    ["Ein Sortieralgorithmus", "Ein Verschlüsselungsverfahren", "Ein Datenbankindex"]),
            ]),
            new("Wissensrepräsentation",
            [
                new("Was ist ein Ziel der Wissensrepräsentation?",
                    "Wissen maschinell verarbeitbar abzubilden",
                    ["Daten möglichst stark zu komprimieren", "Bilder zu rendern", "Netzwerke zu verbinden"]),
                new("Was beschreibt eine Ontologie in der KI?",
                    "Eine formale Beschreibung von Begriffen und ihren Beziehungen",
                    ["Ein neuronales Netz", "Einen Sortieralgorithmus", "Ein Dateiformat"]),
            ]),
            new("Anwendungen der KI",
            [
                new("Was ist ein typisches Anwendungsgebiet der KI?",
                    "Sprachverarbeitung (NLP)",
                    ["Das Formatieren von Festplatten", "Die Stromversorgung von Servern", "Das Verlegen von Netzwerkkabeln"]),
                new("Was leistet ein Empfehlungssystem?",
                    "Es schlägt Nutzern passende Inhalte vor",
                    ["Es verschlüsselt Datenbanken", "Es kompiliert Quellcode", "Es misst die Netzwerklatenz"]),
            ]),
        ],
        ["Maschinelles Lernen"] =
        [
            new("Überwachtes Lernen",
            [
                new("Was kennzeichnet überwachtes Lernen?",
                    "Training mit gelabelten Daten",
                    ["Training ganz ohne Daten", "Training ausschließlich mit ungelabelten Daten", "Lernen nur aus Belohnungssignalen"]),
                new("Was ist ein Beispiel für eine Klassifikationsaufgabe?",
                    "Spam-Erkennung in E-Mails",
                    ["Das Berechnen einer Summe", "Das Sortieren einer Liste", "Das Rendern eines Bildes"]),
            ]),
            new("Unüberwachtes Lernen",
            [
                new("Was ist das Ziel von Clustering?",
                    "Ähnliche Datenpunkte zu Gruppen zusammenfassen",
                    ["Gelabelte Werte vorhersagen", "Modelle zu testen", "Daten zu verschlüsseln"]),
                new("Welches Verfahren gehört zum unüberwachten Lernen?",
                    "k-Means", ["Logistische Regression", "Überwachte Klassifikation", "Lineare Regression"]),
            ]),
            new("Modellbewertung",
            [
                new("Was beschreibt Overfitting?",
                    "Das Modell passt sich zu stark an die Trainingsdaten an",
                    ["Das Modell ist zu einfach", "Die Daten sind verschlüsselt", "Das Training war zu kurz"]),
                new("Wozu dient ein separater Testdatensatz?",
                    "Zur Bewertung der Generalisierung des Modells",
                    ["Zum Trainieren des Modells", "Zum Speichern der Gewichte", "Zum Labeln der Daten"]),
            ]),
        ],
        ["IT-Sicherheit"] =
        [
            new("Kryptografie",
            [
                new("Was kennzeichnet symmetrische Verschlüsselung?",
                    "Der gleiche Schlüssel für Ver- und Entschlüsselung",
                    ["Zwei verschiedene Schlüssel", "Gar kein Schlüssel", "Nur ein öffentlicher Schlüssel"]),
                new("Wozu dient ein Hash-Verfahren?",
                    "Zur Erzeugung eines Prüfwerts fester Länge",
                    ["Zur reversiblen Verschlüsselung", "Zur Datenkomprimierung", "Zur Vergabe von IP-Adressen"]),
            ]),
            new("Angriffsarten",
            [
                new("Was ist ein Phishing-Angriff?",
                    "Das Erschleichen von Daten über gefälschte Nachrichten",
                    ["Das Überlasten eines Servers", "Das Ausnutzen eines Pufferüberlaufs", "Das Abhören von Funkverkehr"]),
                new("Was ist eine SQL-Injection?",
                    "Das Einschleusen von SQL-Code über Eingabefelder",
                    ["Das Verschlüsseln der Datenbank", "Ein reiner Netzwerkscan", "Ein Hardwaredefekt"]),
                new("Was bewirkt ein DDoS-Angriff?",
                    "Er überlastet einen Dienst durch massenhaft Anfragen",
                    ["Er verschlüsselt lokale Dateien", "Er stiehlt Passwörter per Mail", "Er verändert den Quellcode"]),
            ]),
            new("Schutzmaßnahmen",
            [
                new("Wozu dient eine Firewall?",
                    "Zum Filtern des Netzwerkverkehrs",
                    ["Zur Datensicherung", "Zum Rendern von Webseiten", "Zur Verwaltung von Passwörtern"]),
                new("Was erhöht die Sicherheit von Logins zusätzlich zum Passwort?",
                    "Zwei-Faktor-Authentifizierung",
                    ["Ein möglichst kurzes Passwort", "Das Deaktivieren von Updates", "Die Nutzung öffentlicher WLANs"]),
            ]),
        ],
        ["Verteilte Systeme"] =
        [
            new("Grundkonzepte",
            [
                new("Was ist ein verteiltes System?",
                    "Mehrere vernetzte Rechner, die als Einheit wirken",
                    ["Ein einzelner Rechner mit mehreren Programmen", "Eine einzelne CPU", "Eine Festplatte mit mehreren Partitionen"]),
                new("Was bezeichnet Transparenz in verteilten Systemen?",
                    "Die Verteilung wird vor dem Nutzer verborgen",
                    ["Alle Daten sind öffentlich einsehbar", "Das System ist grundsätzlich unverschlüsselt", "Der Quellcode ist sichtbar"]),
            ]),
            new("Kommunikation",
            [
                new("Was ist ein Remote Procedure Call (RPC)?",
                    "Der Aufruf einer Prozedur auf einem entfernten Rechner",
                    ["Ein rein lokaler Funktionsaufruf", "Eine SQL-Abfrage", "Ein Sortierverfahren"]),
                new("Welches Kommunikationsmuster ist für verteilte Systeme typisch?",
                    "Nachrichtenaustausch (Message Passing)",
                    ["Gemeinsamer Speicher auf einem Chip", "Direkte Registerzugriffe", "Interrupt-gesteuerte Hardware"]),
            ]),
            new("Konsistenz",
            [
                new("Was besagt das CAP-Theorem?",
                    "Von Consistency, Availability und Partition Tolerance sind nur zwei gleichzeitig garantierbar",
                    ["Alle drei Eigenschaften sind stets erfüllbar", "Es beschreibt ein Verschlüsselungsverfahren", "Es geht um Sortierkomplexität"]),
                new("Was bedeutet eventual consistency?",
                    "Replikate werden nach einiger Zeit konsistent",
                    ["Daten sind sofort überall identisch", "Daten sind niemals konsistent", "Konsistenz ist prinzipiell unmöglich"]),
            ]),
        ],
        ["Computergrafik"] =
        [
            new("Rasterung und Bilddarstellung",
            [
                new("Was ist ein Pixel?",
                    "Der kleinste adressierbare Bildpunkt",
                    ["Ein dreidimensionales Objekt", "Ein Farbraum", "Ein Dateiformat"]),
                new("Was beschreibt die Auflösung eines Bildes?",
                    "Die Anzahl der Pixel",
                    ["Die Dateigröße in KB", "Die Farbtiefe in Bit", "Die Bildwiederholrate"]),
            ]),
            new("3D-Transformationen",
            [
                new("Womit werden 3D-Transformationen mathematisch beschrieben?",
                    "Mit Matrizen",
                    ["Mit Zeichenketten", "Mit Hashfunktionen", "Mit regulären Ausdrücken"]),
                new("Welche Grundtransformationen gibt es typischerweise?",
                    "Translation, Rotation und Skalierung",
                    ["Addition, Subtraktion und Division", "Insert, Update und Delete", "GET, POST und PUT"]),
            ]),
            new("Beleuchtungsmodelle",
            [
                new("Was modelliert das Phong-Beleuchtungsmodell?",
                    "Ambiente, diffuse und spekulare Lichtanteile",
                    ["Die Netzwerkbandbreite", "Die CPU-Auslastung", "Die Dateikomprimierung"]),
                new("Was ist Ray Tracing?",
                    "Ein Verfahren, das Lichtstrahlen verfolgt",
                    ["Ein Sortieralgorithmus", "Ein Netzwerkprotokoll", "Ein Datenbankindex"]),
            ]),
        ],
        ["Mobile Anwendungen"] =
        [
            new("Plattformen",
            [
                new("Welche Sprache wird für native Android-Apps häufig verwendet?",
                    "Kotlin", ["Swift", "Objective-C", "Ruby"]),
                new("Welche Sprache ist typisch für native iOS-Apps?",
                    "Swift", ["Kotlin", "Java", "PHP"]),
            ]),
            new("App-Architektur",
            [
                new("Welches Architekturmuster ist bei mobilen Apps verbreitet?",
                    "MVVM (Model-View-ViewModel)",
                    ["Das OSI-Modell", "Das relationale Modell", "Die Turingmaschine"]),
                new("Was ist ein Vorteil responsiver Layouts?",
                    "Anpassung an verschiedene Bildschirmgrößen",
                    ["Garantiert höhere Akkulaufzeit", "Schnellere Datenbankabfragen", "Bessere Verschlüsselung"]),
            ]),
            new("Lebenszyklus und Ressourcen",
            [
                new("Was beschreibt der Lebenszyklus einer Activity bzw. App?",
                    "Die Zustände von Start bis Beendigung",
                    ["Die Netzwerktopologie", "Das Datenbankschema", "Den Umfang des Quellcodes"]),
                new("Warum ist sparsamer Ressourceneinsatz auf Mobilgeräten wichtig?",
                    "Wegen begrenzter Akku- und Speicherkapazität",
                    ["Weil kein Prozessor vorhanden ist", "Weil keine Netzverbindung existiert", "Weil Apps sich nie schließen lassen"]),
            ]),
        ],
        ["Cloud Computing"] =
        [
            new("Servicemodelle",
            [
                new("Wofür steht die Abkürzung IaaS?",
                    "Infrastructure as a Service",
                    ["Internet as a Service", "Integration as a Service", "Index as a Service"]),
                new("Was stellt SaaS bereit?",
                    "Fertige Anwendungen über das Internet",
                    ["Nur virtuelle Maschinen", "Nur Netzwerk-Hardware", "Nur reinen Speicherplatz"]),
            ]),
            new("Virtualisierung",
            [
                new("Was ist ein Vorteil von Containern gegenüber klassischen VMs?",
                    "Geringerer Overhead durch geteilten Kernel",
                    ["Sie benötigen mehr Speicher", "Sie starten deutlich langsamer", "Sie brauchen jeweils eigene Hardware"]),
                new("Was leistet ein Hypervisor?",
                    "Er verwaltet mehrere virtuelle Maschinen",
                    ["Er rendert Grafik", "Er kompiliert Quellcode", "Er filtert E-Mails"]),
            ]),
            new("Skalierung",
            [
                new("Was bedeutet horizontale Skalierung?",
                    "Das Hinzufügen weiterer Instanzen",
                    ["Das Aufrüsten einer einzelnen Maschine", "Das Löschen von Daten", "Das Verschlüsseln des Traffics"]),
                new("Was versteht man unter Elastizität in der Cloud?",
                    "Das automatische Anpassen der Ressourcen an die Last",
                    ["Ein fest vorgegebenes Ressourcenlimit", "Die physische Verkabelung", "Ein reines Backup-Verfahren"]),
            ]),
        ],
        ["Datenanalyse"] =
        [
            new("Deskriptive Statistik",
            [
                new("Was gibt der Median an?",
                    "Den mittleren Wert einer sortierten Verteilung",
                    ["Den häufigsten Wert", "Das arithmetische Mittel", "Die Spannweite"]),
                new("Was beschreibt die Standardabweichung?",
                    "Die Streuung der Daten um den Mittelwert",
                    ["Den größten Einzelwert", "Die Anzahl der Werte", "Den Median"]),
            ]),
            new("Datenvisualisierung",
            [
                new("Welches Diagramm eignet sich am besten für Anteile an einem Ganzen?",
                    "Kreisdiagramm",
                    ["Liniendiagramm für Zeitreihen", "Streudiagramm", "Netzwerkdiagramm"]),
                new("Wofür eignet sich ein Histogramm?",
                    "Zur Darstellung von Häufigkeitsverteilungen",
                    ["Zur Anzeige prozentualer Anteile", "Zur Netzwerkanalyse", "Zur Codeformatierung"]),
            ]),
            new("Datenaufbereitung",
            [
                new("Was versteht man unter Data Cleaning?",
                    "Das Bereinigen fehlerhafter oder fehlender Daten",
                    ["Das Verschlüsseln der Daten", "Das Rendern von Diagrammen", "Das Komprimieren von Bildern"]),
                new("Warum ist die Datenaufbereitung wichtig?",
                    "Schlechte Daten führen zu falschen Analyseergebnissen",
                    ["Sie beschleunigt die CPU", "Sie ersetzt die Datenbank", "Sie ist gesetzlich vorgeschrieben zu unterlassen"]),
            ]),
        ],
        ["Mensch-Computer-Interaktion"] =
        [
            new("Usability",
            [
                new("Was beschreibt Usability?",
                    "Die Gebrauchstauglichkeit eines Systems",
                    ["Die Netzwerkbandbreite", "Die Größe des Quellcodes", "Die Farbtiefe des Displays"]),
                new("Was ist ein Usability-Test?",
                    "Das Beobachten von Nutzern bei der Bedienung",
                    ["Ein Lasttest des Servers", "Ein Verschlüsselungsverfahren", "Ein Datenbank-Backup"]),
            ]),
            new("Interaktionsdesign",
            [
                new("Was ist ein Mental Model der Nutzer?",
                    "Die Vorstellung, wie ein System funktioniert",
                    ["Der Quellcode des Systems", "Das Datenbankschema", "Die CPU-Architektur"]),
                new("Warum ist konsistentes Design wichtig?",
                    "Es erleichtert das Erlernen der Bedienung",
                    ["Es erhöht die Taktfrequenz", "Es verschlüsselt Daten", "Es spart Netzwerkbandbreite"]),
            ]),
            new("Barrierefreiheit",
            [
                new("Was ist ein Ziel von Barrierefreiheit (Accessibility)?",
                    "Nutzbarkeit auch für Menschen mit Einschränkungen",
                    ["Eine höhere Serverlast", "Kleinere Dateien", "Schnellere Datenbanken"]),
                new("Was hilft sehbehinderten Nutzern beim Bedienen von Software?",
                    "Ausreichender Kontrast und Screenreader-Unterstützung",
                    ["Sehr kleine graue Schrift", "Ausschließliche Maussteuerung", "Stark blinkende Elemente"]),
            ]),
        ],
        ["Compilerbau"] =
        [
            new("Lexikalische Analyse",
            [
                new("Was macht ein Lexer (Scanner)?",
                    "Er zerlegt den Quelltext in Tokens",
                    ["Er erzeugt direkt Maschinencode", "Er optimiert Schleifen", "Er bindet Bibliotheken ein"]),
                new("Was ist ein Token?",
                    "Eine kleinste bedeutungstragende Einheit des Quelltexts",
                    ["Eine ganze Quelldatei", "Ein Compilerfehler", "Eine CPU-Instruktion"]),
            ]),
            new("Syntaxanalyse",
            [
                new("Was prüft der Parser?",
                    "Ob die Tokenfolge der Grammatik entspricht",
                    ["Ob die Festplatte voll ist", "Ob das Netzwerk verfügbar ist", "Ob die Datei existiert"]),
                new("Was erzeugt der Parser typischerweise?",
                    "Einen Syntaxbaum (AST)",
                    ["Eine SQL-Tabelle", "Ein Bitmap-Bild", "Ein Netzwerkpaket"]),
            ]),
            new("Codeerzeugung",
            [
                new("Was ist eine Aufgabe der Codeoptimierung?",
                    "Effizienteren Zielcode zu erzeugen",
                    ["Den Quelltext einzulesen", "Tokens zu bilden", "Die Grammatik zu definieren"]),
                new("Was ist Zwischencode?",
                    "Eine plattformunabhängige Darstellung zwischen Quell- und Maschinencode",
                    ["Der finale Maschinencode", "Der unveränderte Originalquelltext", "Ein Quellcode-Kommentar"]),
            ]),
        ],
        ["Mikrocontroller-Programmierung"] =
        [
            new("Hardwaregrundlagen",
            [
                new("Was ist charakteristisch für einen Mikrocontroller?",
                    "CPU, Speicher und Peripherie auf einem Chip",
                    ["Ein reiner Grafikprozessor", "Ein Netzwerkrouter", "Eine externe Festplatte"]),
                new("Warum ist der Speicher auf Mikrocontrollern oft knapp?",
                    "Wegen der kompakten, kostengünstigen Bauweise",
                    ["Weil sie keine CPU besitzen", "Weil sie nur im Netzwerk laufen", "Weil Speicher technisch unmöglich ist"]),
            ]),
            new("GPIO und Peripherie",
            [
                new("Wofür steht die Abkürzung GPIO?",
                    "General Purpose Input/Output",
                    ["Graphics Processing Input Output", "Global Program Interface Option", "General Protocol Internet Output"]),
                new("Wozu dient ein Analog-Digital-Wandler (ADC)?",
                    "Analoge Signale in digitale Werte umzuwandeln",
                    ["Digitale Werte auf einem Display anzuzeigen", "Strom zu erzeugen", "Daten zu verschlüsseln"]),
            ]),
            new("Echtzeitverhalten",
            [
                new("Was kennzeichnet ein Echtzeitsystem?",
                    "Es garantiert Reaktionen innerhalb fester Zeitschranken",
                    ["Es ist immer besonders schnell", "Es benötigt kein Timing", "Es läuft ausschließlich in der Cloud"]),
                new("Wozu dient ein Interrupt?",
                    "Zur sofortigen Reaktion auf ein Ereignis",
                    ["Zum dauerhaften Abschalten der CPU", "Zum Sortieren von Daten", "Zum Rendern von Grafik"]),
            ]),
        ],
        ["Diskrete Mathematik"] =
        [
            new("Mengenlehre",
            [
                new("Was ist die Schnittmenge zweier Mengen A und B?",
                    "Die Elemente, die in A und in B liegen",
                    ["Alle Elemente von A und B zusammen", "Nur die Elemente von A", "Immer die leere Menge"]),
                new("Was bezeichnet die Vereinigungsmenge von A und B?",
                    "Alle Elemente, die in A oder in B liegen",
                    ["Nur die gemeinsamen Elemente", "Die Differenz der Mengen", "Die Potenzmenge von A"]),
            ]),
            new("Aussagenlogik",
            [
                new("Wann ist eine Konjunktion (A ∧ B) wahr?",
                    "Wenn A und B beide wahr sind",
                    ["Wenn mindestens eine Aussage wahr ist", "Wenn beide falsch sind", "In jedem Fall"]),
                new("Wann ist eine Disjunktion (A ∨ B) wahr?",
                    "Wenn mindestens eine Aussage wahr ist",
                    ["Nur wenn beide wahr sind", "Nur wenn beide falsch sind", "Niemals"]),
            ]),
            new("Graphentheorie",
            [
                new("Was ist der Grad eines Knotens in einem Graphen?",
                    "Die Anzahl der Kanten an diesem Knoten",
                    ["Die Anzahl aller Knoten", "Die Länge des Graphen", "Die Anzahl der Zusammenhangskomponenten"]),
                new("Was ist ein Baum in der Graphentheorie?",
                    "Ein zusammenhängender, kreisfreier Graph",
                    ["Ein Graph mit mindestens einem Zyklus", "Ein vollständiger Graph", "Ein gerichteter Kreis"]),
            ]),
        ],
    };

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

            var alleFragenDerLv = new List<MCFrage>();

            if (Kurskatalog.TryGetValue(titel, out var kapitelVorlagen))
            {
                // Praxisnahe Kapitel und Fragen aus dem kuratierten Katalog.
                for (var k = 0; k < kapitelVorlagen.Length; k++)
                {
                    var vorlage = kapitelVorlagen[k];
                    var kapitel = new Kapitel
                    {
                        Titel = vorlage.Titel,
                        Kapitelnummer = k + 1
                    };

                    foreach (var frageVorlage in vorlage.Fragen)
                    {
                        var frage = ErzeugeFrage(frageVorlage, rng);
                        kapitel.MCFragen.Add(frage);
                        alleFragenDerLv.Add(frage);
                    }

                    lv.Kapitel.Add(kapitel);
                }
            }
            else
            {
                // Fallback für Themen ohne Katalogeintrag: generische Kapitel.
                var anzahlKapitel = rng.Next(2, 4);
                for (var k = 0; k < anzahlKapitel; k++)
                {
                    var kapitel = new Kapitel
                    {
                        Titel = $"{Kapiteltitel[k % Kapiteltitel.Length]} – {titel}",
                        Kapitelnummer = k + 1
                    };

                    var anzahlFragen = rng.Next(2, 4);
                    for (var f = 0; f < anzahlFragen; f++)
                    {
                        var frage = ErzeugeGenerischeFrage(titel, k + 1, f + 1, rng);
                        kapitel.MCFragen.Add(frage);
                        alleFragenDerLv.Add(frage);
                    }

                    lv.Kapitel.Add(kapitel);
                }
            }

            // 1 Prüfung pro Lehrveranstaltung (übersichtlich halten)
            var pruefung = new Pruefung
            {
                // Termine rund um das aktuelle Datum verteilen
                Datum = DateTime.Today.AddDays(rng.Next(-120, 121)).AddHours(9)
            };

            // Zufällige Teilmenge der Fragen dieser LV der Prüfung zuordnen
            var fragenFuerPruefung = alleFragenDerLv
                .OrderBy(_ => rng.Next())
                .Take(Math.Min(alleFragenDerLv.Count, rng.Next(3, 6)))
                .ToList();

            foreach (var frage in fragenFuerPruefung)
            {
                pruefung.PruefungMCFragen.Add(new PruefungMCFrage { MCFrage = frage });
            }

            lv.Pruefungen.Add(pruefung);
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

    /// <summary>
    /// Baut eine MC-Frage aus einer Katalogvorlage. Die richtige und die
    /// falschen Antworten werden gemischt, damit die korrekte Option nicht
    /// immer an derselben Stelle steht.
    /// </summary>
    private static MCFrage ErzeugeFrage(FrageVorlage vorlage, Random rng)
    {
        var frage = new MCFrage { Fragentext = vorlage.Frage };

        var optionen = new List<MCAntwortOption>
        {
            new() { Antworttext = vorlage.Richtig, IstRichtig = true }
        };
        foreach (var falsch in vorlage.Falsch)
        {
            optionen.Add(new MCAntwortOption { Antworttext = falsch, IstRichtig = false });
        }

        foreach (var option in optionen.OrderBy(_ => rng.Next()))
        {
            frage.AntwortOptionen.Add(option);
        }

        return frage;
    }

    /// <summary>
    /// Generischer Fallback-Fragengenerator für Themen ohne Katalogeintrag.
    /// </summary>
    private static MCFrage ErzeugeGenerischeFrage(string thema, int kapitelNr, int frageNr, Random rng)
    {
        var frage = new MCFrage
        {
            Fragentext = $"{thema} – Kapitel {kapitelNr}, Frage {frageNr}: Welche Aussage ist korrekt?"
        };

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