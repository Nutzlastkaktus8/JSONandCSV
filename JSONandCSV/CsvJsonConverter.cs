using System.Text;

namespace JSONandCSV;

/// <summary>
/// Konvertiert Objekte zwischen CSV und Listen.
/// </summary>
public static class CsvJsonConverter
{
    /// <summary>
    /// Konvertiert eine Liste von Objekten in einen CSV-String.
    /// </summary>
    public static string ToCsv<T>(List<T> items, char delimiter = ';')
    {
        if (items == null || items.Count == 0)
            return string.Empty;

        var properties = typeof(T).GetProperties();
        var sb = new StringBuilder();

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

    /// <summary>
    /// Konvertiert einen CSV-String in eine Liste von Objekten.
    /// </summary>
    public static List<T> FromCsv<T>(string csv, char delimiter = ';') where T : class
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
}
