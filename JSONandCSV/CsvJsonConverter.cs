using System.Text;
using System.Text.Json;

namespace JSONandCSV;

/// <summary>
/// Konvertiert zwischen JSON und CSV Formaten.
/// </summary>
public static class CsvJsonConverter
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    #region JSON <-> CSV (Hauptfunktionalität)

    /// <summary>
    /// Konvertiert einen JSON-String direkt in einen CSV-String.
    /// </summary>
    public static string JsonToCsv(string json, char delimiter = ';')
    {
        if (string.IsNullOrWhiteSpace(json))
            return string.Empty;

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
            return string.Empty;

        var sb = new StringBuilder();
        var firstElement = root[0];

        // Header aus den Property-Namen des ersten Objekts erstellen
        var properties = firstElement.EnumerateObject().Select(p => p.Name).ToList();
        sb.AppendLine(string.Join(delimiter, properties));

        // Datenzeilen erstellen
        foreach (var element in root.EnumerateArray())
        {
            var values = new List<string>();
            foreach (var propName in properties)
            {
                if (element.TryGetProperty(propName, out var prop))
                {
                    values.Add(prop.ValueKind == JsonValueKind.String 
                        ? prop.GetString() ?? string.Empty 
                        : prop.GetRawText());
                }
                else
                {
                    values.Add(string.Empty);
                }
            }
            sb.AppendLine(string.Join(delimiter, values));
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Konvertiert einen CSV-String direkt in einen JSON-String.
    /// </summary>
    public static string CsvToJson(string csv, char delimiter = ';')
    {
        if (string.IsNullOrWhiteSpace(csv))
            return "[]";

        using var reader = new StringReader(csv);

        var headerLine = reader.ReadLine();
        if (headerLine == null)
            return "[]";

        var headers = headerLine.Split(delimiter);
        var result = new List<Dictionary<string, object>>();

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = line.Split(delimiter);
            var obj = new Dictionary<string, object>();

            for (int i = 0; i < headers.Length && i < values.Length; i++)
            {
                // Versuche numerische Werte zu erkennen
                if (int.TryParse(values[i], out int intVal))
                    obj[headers[i]] = intVal;
                else if (double.TryParse(values[i], out double doubleVal))
                    obj[headers[i]] = doubleVal;
                else if (bool.TryParse(values[i], out bool boolVal))
                    obj[headers[i]] = boolVal;
                else
                    obj[headers[i]] = values[i];
            }

            result.Add(obj);
        }

        return JsonSerializer.Serialize(result, JsonOptions);
    }

    #endregion

    #region Objekt-basierte Hilfsmethoden

    /// <summary>
    /// Konvertiert eine Liste von Objekten in einen CSV-String.
    /// </summary>
    public static string ToCsv<T>(List<T> items, char delimiter = ';')
    {
        if (items == null || items.Count == 0)
            return string.Empty;

        // Über JSON-Zwischenschritt für Konsistenz
        string json = JsonSerializer.Serialize(items);
        return JsonToCsv(json, delimiter);
    }

    /// <summary>
    /// Konvertiert einen CSV-String in eine Liste von Objekten.
    /// </summary>
    public static List<T> FromCsv<T>(string csv, char delimiter = ';') where T : class
    {
        // Über JSON-Zwischenschritt für Konsistenz
        string json = CsvToJson(csv, delimiter);
        return JsonSerializer.Deserialize<List<T>>(json) ?? [];
    }

    #endregion
}
