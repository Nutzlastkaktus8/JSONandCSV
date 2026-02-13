using JSONandCSV;

namespace TestProject;

public class CsvJsonConverterTests
{
    #region JsonToCsv Tests

    [Fact]
    public void JsonToCsv_WithValidJson_ReturnsCorrectCsv()
    {
        // Arrange
        string json = """
            [
                {"Vorname":"Max","Nachname":"Mustermann","Alter":30,"Email":"max@example.com"},
                {"Vorname":"Anna","Nachname":"Schmidt","Alter":25,"Email":"anna@example.com"}
            ]
            """;

        // Act
        string csv = CsvJsonConverter.JsonToCsv(json, ';');

        // Assert
        var lines = csv.Split(Environment.NewLine);
        Assert.Equal(3, lines.Length);
        Assert.Equal("Vorname;Nachname;Alter;Email", lines[0]);
        Assert.Equal("Max;Mustermann;30;max@example.com", lines[1]);
        Assert.Equal("Anna;Schmidt;25;anna@example.com", lines[2]);
    }

    [Fact]
    public void JsonToCsv_WithEmptyArray_ReturnsEmptyString()
    {
        // Act
        string csv = CsvJsonConverter.JsonToCsv("[]");

        // Assert
        Assert.Equal(string.Empty, csv);
    }

    [Fact]
    public void JsonToCsv_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        string csv = CsvJsonConverter.JsonToCsv(string.Empty);

        // Assert
        Assert.Equal(string.Empty, csv);
    }

    [Theory]
    [InlineData(';')]
    [InlineData(',')]
    [InlineData('\t')]
    public void JsonToCsv_WithDifferentDelimiters_UsesCorrectDelimiter(char delimiter)
    {
        // Arrange
        string json = """[{"Name":"Test","Value":123}]""";

        // Act
        string csv = CsvJsonConverter.JsonToCsv(json, delimiter);

        // Assert
        var headerParts = csv.Split(Environment.NewLine)[0].Split(delimiter);
        Assert.Equal(2, headerParts.Length);
    }

    #endregion

    #region CsvToJson Tests

    [Fact]
    public void CsvToJson_WithValidCsv_ReturnsCorrectJson()
    {
        // Arrange
        string csv = "Vorname;Nachname;Alter;Email\nMax;Mustermann;30;max@example.com";

        // Act
        string json = CsvJsonConverter.CsvToJson(csv, ';');

        // Assert
        Assert.Contains("\"Vorname\": \"Max\"", json);
        Assert.Contains("\"Nachname\": \"Mustermann\"", json);
        Assert.Contains("\"Alter\": 30", json); // Numerisch erkannt
        Assert.Contains("\"Email\": \"max@example.com\"", json);
    }

    [Fact]
    public void CsvToJson_WithEmptyString_ReturnsEmptyArray()
    {
        // Act
        string json = CsvJsonConverter.CsvToJson(string.Empty);

        // Assert
        Assert.Equal("[]", json);
    }

    [Fact]
    public void CsvToJson_WithOnlyHeader_ReturnsEmptyArray()
    {
        // Arrange
        string csv = "Vorname;Nachname;Alter;Email";

        // Act
        string json = CsvJsonConverter.CsvToJson(csv);

        // Assert
        Assert.Equal("[]", json);
    }

    [Theory]
    [InlineData(';')]
    [InlineData(',')]
    public void CsvToJson_WithDifferentDelimiters_ParsesCorrectly(char delimiter)
    {
        // Arrange
        string csv = $"Name{delimiter}Value\nTest{delimiter}123";

        // Act
        string json = CsvJsonConverter.CsvToJson(csv, delimiter);

        // Assert
        Assert.Contains("\"Name\": \"Test\"", json);
        Assert.Contains("\"Value\": 123", json);
    }

    #endregion

    #region Roundtrip Tests

    [Fact]
    public void Roundtrip_JsonToCsvAndBack_PreservesData()
    {
        // Arrange
        string originalJson = """
            [
                {"Vorname":"Max","Nachname":"Mustermann","Alter":30,"Email":"max@example.com"},
                {"Vorname":"Anna","Nachname":"Schmidt","Alter":25,"Email":"anna@example.com"}
            ]
            """;

        // Act
        string csv = CsvJsonConverter.JsonToCsv(originalJson, ';');
        string restoredJson = CsvJsonConverter.CsvToJson(csv, ';');

        // Assert
        Assert.Contains("\"Vorname\": \"Max\"", restoredJson);
        Assert.Contains("\"Vorname\": \"Anna\"", restoredJson);
        Assert.Contains("\"Alter\": 30", restoredJson);
        Assert.Contains("\"Alter\": 25", restoredJson);
    }

    [Fact]
    public void Roundtrip_CsvToJsonAndBack_PreservesData()
    {
        // Arrange
        string originalCsv = $"Name;Alter;Aktiv{Environment.NewLine}Max;30;true{Environment.NewLine}Anna;25;false";

        // Act
        string json = CsvJsonConverter.CsvToJson(originalCsv, ';');
        string restoredCsv = CsvJsonConverter.JsonToCsv(json, ';');

        // Assert
        var originalLines = originalCsv.Split(Environment.NewLine);
        var restoredLines = restoredCsv.Split(Environment.NewLine);
        Assert.Equal(originalLines.Length, restoredLines.Length);
        Assert.Equal(originalLines[0], restoredLines[0]); // Header
    }

    #endregion

    #region ToCsv/FromCsv (Objekt-basiert) Tests

    [Fact]
    public void ToCsv_WithValidList_ReturnsCorrectCsv()
    {
        // Arrange
        var personen = new List<Person>
        {
            new("Max", "Mustermann", 30, "max@example.com"),
            new("Anna", "Schmidt", 25, "anna@example.com")
        };

        // Act
        string csv = CsvJsonConverter.ToCsv(personen, ';');

        // Assert
        var lines = csv.Split(Environment.NewLine);
        Assert.Equal(3, lines.Length);
        Assert.Contains("Vorname", lines[0]);
        Assert.Contains("Max", lines[1]);
    }

    [Fact]
    public void FromCsv_WithValidCsv_ReturnsCorrectObjects()
    {
        // Arrange
        string csv = "Vorname;Nachname;Alter;Email\nMax;Mustermann;30;max@example.com";

        // Act
        var result = CsvJsonConverter.FromCsv<Person>(csv, ';');

        // Assert
        Assert.Single(result);
        Assert.Equal("Max", result[0].Vorname);
        Assert.Equal(30, result[0].Alter);
    }

    #endregion

    #region CultureInfo Tests

    [Fact]
    public void CsvToJson_WithDecimalNumbers_ParsesInvariantCultureCorrectly()
    {
        // Arrange - Dezimalzahlen mit Punkt (unabhängig von System-Kultur)
        string csv = "Name;Price;Discount\nProduct;19.99;2.5";

        // Act
        string json = CsvJsonConverter.CsvToJson(csv, ';');

        // Assert
        Assert.Contains("\"Price\": 19.99", json);
        Assert.Contains("\"Discount\": 2.5", json);
    }

    [Fact]
    public void CsvToJson_WithGermanDecimalFormat_DoesNotParseAsNumber()
    {
        // Arrange - Deutsche Zahlenformat (Komma) soll NICHT als Zahl erkannt werden
        string csv = "Name;Price\nProduct;19,99";

        // Act
        string json = CsvJsonConverter.CsvToJson(csv, ';');

        // Assert - Sollte als String bleiben, da InvariantCulture verwendet wird
        Assert.Contains("\"Price\": \"19,99\"", json);
    }

    [Fact]
    public void CsvToJson_WithLargeNumbersAndThousandsSeparator_ParsesCorrectly()
    {
        // Arrange - Zahlen mit Tausendertrennzeichen
        string csv = "Name;Population\nCity;1000000";

        // Act
        string json = CsvJsonConverter.CsvToJson(csv, ';');

        // Assert
        Assert.Contains("\"Population\": 1000000", json);
    }

    [Fact]
    public void CsvToJson_WithEmptyValues_ConvertsToNull()
    {
        // Arrange - Leere Werte und Whitespace
        string csv = "Name;Age;Email\nMax;30;\nAnna;;anna@test.com";

        // Act
        string json = CsvJsonConverter.CsvToJson(csv, ';');

        // Assert
        Assert.Contains("\"Email\": null", json);
        Assert.Contains("\"Age\": null", json);
    }

    [Theory]
    [InlineData("123", "123")]           // Integer
    [InlineData("123.45", "123.45")]     // Double
    [InlineData("true", "true")]         // Boolean
    [InlineData("false", "false")]       // Boolean
    [InlineData("  42  ", "42")]         // Integer mit Whitespace
    [InlineData("text", "\"text\"")]     // String
    public void CsvToJson_WithVariousTypes_InfersCorrectType(string input, string expectedInJson)
    {
        // Arrange
        string csv = $"Value\n{input}";

        // Act
        string json = CsvJsonConverter.CsvToJson(csv, ';');

        // Assert
        Assert.Contains($"\"Value\": {expectedInJson}", json);
    }

    [Fact]
    public void CsvToJson_WithNegativeNumbers_ParsesCorrectly()
    {
        // Arrange
        string csv = "Temperature;Balance\n-15;-123.45";

        // Act
        string json = CsvJsonConverter.CsvToJson(csv, ';');

        // Assert
        Assert.Contains("\"Temperature\": -15", json);
        Assert.Contains("\"Balance\": -123.45", json);
    }

    [Fact]
    public void CsvToJson_WithScientificNotation_ParsesCorrectly()
    {
        // Arrange
        string csv = "Value\n1.5E+3";

        // Act
        string json = CsvJsonConverter.CsvToJson(csv, ';');

        // Assert
        Assert.Contains("\"Value\": 1500", json);
    }

    #endregion
}
