using System.Text.Json;

// Beispiel-Datenklasse
var personen = new List<Person>
{
    new("Max", "Mustermann", 30, "max@example.com"),
    new("Anna", "Schmidt", 25, "anna@example.com"),
    new("Peter", "Müller", 45, "peter@example.com")
};

Console.WriteLine("=== OBJEKT -> JSON ===");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
string json = JsonSerializer.Serialize(personen, jsonOptions);
Console.WriteLine(json);

Console.WriteLine("\n=== JSON -> CSV ===");
string csv = JsonToCsv(personen);
Console.WriteLine(csv);

Console.WriteLine("\n=== CSV -> JSON ===");
var personenAusCsv = CsvToObjects<Person>(csv);
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
static string JsonToCsv<T>(List<T> items)
{
    if (items == null || items.Count == 0)
        return string.Empty;

    var properties = typeof(T).GetProperties();
    var header = string.Join(";", properties.Select(p => p.Name));
    var lines = new List<string> { header };

    foreach (var item in items)
    {
        var values = properties.Select(p => p.GetValue(item)?.ToString() ?? string.Empty);
        lines.Add(string.Join(";", values));
    }

    return string.Join(Environment.NewLine, lines);
}

static List<T> CsvToObjects<T>(string csv) where T : class
{
    var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    var result = new List<T>();

    if (lines.Length < 2)
        return result;

    var headers = lines[0].Split(';');
    var constructor = typeof(T).GetConstructors().First();
    var parameters = constructor.GetParameters();

    foreach (var line in lines.Skip(1))
    {
        var values = line.Split(';');
        var args = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var headerIndex = Array.FindIndex(headers, h => string.Equals(h, parameters[i].Name, StringComparison.OrdinalIgnoreCase));
            if (headerIndex >= 0 && headerIndex < values.Length)
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
