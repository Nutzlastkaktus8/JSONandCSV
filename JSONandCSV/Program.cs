using System.Globalization;
using System.Text.Json;
using JSONandCSV;

// CSV-Trennzeichen basierend auf der aktuellen Kultur ermitteln
// (z.B. ";" für DE/AT, "," für US/UK)
char csvDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];
Console.WriteLine($"Erkanntes regionales CSV-Trennzeichen: '{csvDelimiter}'\n");

// Simuliere eine JSON-Quelle (z.B. von einer API, Datei, etc.)
string jsonInput = """
    [
        {"Vorname":"Max","Nachname":"Mustermann","Alter":30,"Email":"max@example.com"},
        {"Vorname":"Anna","Nachname":"Schmidt","Alter":25,"Email":"anna@example.com"},
        {"Vorname":"Peter","Nachname":"Mueller","Alter":45,"Email":"peter@example.com"}
    ]
    """;

Console.WriteLine("=== EINGABE: JSON ===");
Console.WriteLine(jsonInput);

Console.WriteLine("\n=== JSON -> CSV ===");
string csv = CsvJsonConverter.JsonToCsv(jsonInput, csvDelimiter);
Console.WriteLine(csv);

Console.WriteLine("\n=== CSV -> JSON ===");
string jsonOutput = CsvJsonConverter.CsvToJson(csv, csvDelimiter);
Console.WriteLine(jsonOutput);

Console.WriteLine("\n=== JSON -> OBJEKT (optional) ===");
var personen = JsonSerializer.Deserialize<List<Person>>(jsonOutput);
Console.WriteLine("Deserialisierte Objekte:");
foreach (var p in personen!)
{
    Console.WriteLine($"  - {p.Vorname} {p.Nachname}, {p.Alter} Jahre, {p.Email}");
}
