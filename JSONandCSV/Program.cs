using System.Globalization;
using System.Text.Json;
using JSONandCSV;

// CSV-Trennzeichen basierend auf der aktuellen Kultur ermitteln
// (z.B. ";" für DE/AT, "," für US/UK)
char csvDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];
Console.WriteLine($"Erkanntes regionales CSV-Trennzeichen: '{csvDelimiter}'\n");

// Oder manuell setzen:
// char csvDelimiter = ';';
// char csvDelimiter = ',';

// Beispiel-Datenklasse     
List<Person> personen =
[
    new("Max", "Mustermann", 30, "max@example.com"),
    new("Anna", "Schmidt", 25, "anna@example.com"),
    new("Peter", "Mueller", 45, "peter@example.com")
];

Console.WriteLine("=== OBJEKT -> JSON ===");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
string json = JsonSerializer.Serialize(personen, jsonOptions);
Console.WriteLine(json);

Console.WriteLine("\n=== JSON -> CSV ===");
string csv = CsvJsonConverter.ToCsv(personen, csvDelimiter);
Console.WriteLine(csv);

Console.WriteLine("\n=== CSV -> JSON ===");
var personenAusCsv = CsvJsonConverter.FromCsv<Person>(csv, csvDelimiter);
string jsonAusCsv = JsonSerializer.Serialize(personenAusCsv, jsonOptions);
Console.WriteLine(jsonAusCsv);

Console.WriteLine("\n=== JSON -> OBJEKT ===");
var wiederhergestelltePersonen = JsonSerializer.Deserialize<List<Person>>(jsonAusCsv);
Console.WriteLine("Wiederhergestellte Objekte:");
foreach (var p in wiederhergestelltePersonen!)
{
    Console.WriteLine($"  - {p.Vorname} {p.Nachname}, {p.Alter} Jahre, {p.Email}");
}
