using System.Globalization;
using System.Text.Json;

// CSV-Trennzeichen basierend auf der aktuellen Kultur ermitteln
// (z.B. ";" für DE/AT, "," für US/UK)
char csvDelimiter = CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];
Console.WriteLine($"Erkanntes regionales CSV-Trennzeichen: '{csvDelimiter}'\n");

// Oder manuell setzen:
// char csvDelimiter = ';';
// char csvDelimiter = ',';

// Beispiel-Datenklasse     
List<Person>? personen = new List<Person>
{
    new("Max", "Mustermann", 30, "max@example.com"),
    new("Anna", "Schmidt", 25, "anna@example.com"),
    new("Peter", "Mueller", 45, "peter@example.com")
};

Console.WriteLine("=== OBJEKT -> JSON ===");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
string json = JsonSerializer.Serialize(personen, jsonOptions);
Console.WriteLine(json);

Console.WriteLine("\n=== JSON -> CSV ===");
string csv = JsonToCsv(personen, csvDelimiter);
Console.WriteLine(csv);

Console.WriteLine("\n=== CSV -> JSON ===");
var personenAusCsv = CsvToObjects<Person>(csv, csvDelimiter);
string jsonAusCsv = JsonSerializer.Serialize(personenAusCsv, jsonOptions);
Console.WriteLine(jsonAusCsv);

Console.WriteLine("\n=== JSON -> OBJEKT ===");
var wiederhergestelltePersonen = JsonSerializer.Deserialize<List<Person>>(jsonAusCsv);
Console.WriteLine("Wiederhergestellte Objekte:");
foreach (var p in wiederhergestelltePersonen!)
{
    Console.WriteLine($"  - {p.Vorname} {p.Nachname}, {p.Alter} Jahre, {p.Email}");
}

// Hilfsmethoden
static string JsonToCsv<T>(List<T> items, char delimiter = ';')
{
    if (items == null || items.Count == 0)
        return string.Empty;

    var properties = typeof(T).GetProperties();
    var sb = new System.Text.StringBuilder();

    // Header-Zeile erstellen
    for (int i = 0; i < properties.Length; i++)
    {
        if (i > 0) sb.Append(delimiter);
        sb.Append(properties[i].Name);
    }
    sb.AppendLine();

    // Datenzeilen erstellen
    foreach (var item in items)
    {
        for (int i = 0; i < properties.Length; i++)
        {
            if (i > 0) sb.Append(delimiter);
            sb.Append(properties[i].GetValue(item)?.ToString() ?? string.Empty);
        }
        sb.AppendLine();
    }

    return sb.ToString().TrimEnd();
}

static List<T> CsvToObjects<T>(string csv, char delimiter = ';') where T : class
{
    var result = new List<T>();
    if (string.IsNullOrEmpty(csv))
        return result;

    using var reader = new StringReader(csv);

    // Header-Zeile lesen und in Dictionary für O(1) Lookup umwandeln
    var headerLine = reader.ReadLine();
    if (headerLine == null)
        return result;

    var headers = headerLine.Split(delimiter);
    var headerIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    for (int i = 0; i < headers.Length; i++)
    {
        headerIndexMap[headers[i]] = i;
    }

    // Konstruktor und Parameter einmalig ermitteln
    var constructor = typeof(T).GetConstructors().FirstOrDefault();
    if (constructor == null)
        return result;

    var parameters = constructor.GetParameters();

    // Datenzeilen verarbeiten
    string? line;
    while ((line = reader.ReadLine()) != null)
    {
        if (string.IsNullOrWhiteSpace(line))
            continue;

        var values = line.Split(delimiter);
        var args = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            // Dictionary-Lookup statt Array.FindIndex (O(1) statt O(n))
            if (headerIndexMap.TryGetValue(parameters[i].Name!, out int headerIndex) 
                && headerIndex < values.Length)
            {
                args[i] = Convert.ChangeType(values[headerIndex], parameters[i].ParameterType);
            }
        }

        result.Add((T)constructor.Invoke(args));
    }

    return result;
}

// Datenmodell
record Person(string Vorname, string Nachname, int Alter, string Email);
