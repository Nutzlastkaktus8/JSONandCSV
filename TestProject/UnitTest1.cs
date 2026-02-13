using JSONandCSV;

namespace TestProject;

public class CsvJsonConverterTests
{
    #region ToCsv Tests

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
        Assert.Equal("Vorname;Nachname;Alter;Email", lines[0]);
        Assert.Equal("Max;Mustermann;30;max@example.com", lines[1]);
        Assert.Equal("Anna;Schmidt;25;anna@example.com", lines[2]);
    }

    [Fact]
    public void ToCsv_WithEmptyList_ReturnsEmptyString()
    {
        // Arrange
        var emptyList = new List<Person>();

        // Act
        string csv = CsvJsonConverter.ToCsv(emptyList);

        // Assert
        Assert.Equal(string.Empty, csv);
    }

    [Fact]
    public void ToCsv_WithNullList_ReturnsEmptyString()
    {
        // Act
        string csv = CsvJsonConverter.ToCsv<Person>(null!);

        // Assert
        Assert.Equal(string.Empty, csv);
    }

    [Theory]
    [InlineData(';')]
    [InlineData(',')]
    [InlineData('\t')]
    public void ToCsv_WithDifferentDelimiters_UsesCorrectDelimiter(char delimiter)
    {
        // Arrange
        var personen = new List<Person> { new("Max", "Mustermann", 30, "max@example.com") };

        // Act
        string csv = CsvJsonConverter.ToCsv(personen, delimiter);

        // Assert
        Assert.Contains(delimiter.ToString(), csv);
        var headerParts = csv.Split(Environment.NewLine)[0].Split(delimiter);
        Assert.Equal(4, headerParts.Length);
    }

    #endregion

    #region FromCsv Tests

    [Fact]
    public void FromCsv_WithValidCsv_ReturnsCorrectObjects()
    {
        // Arrange
        string csv = "Vorname;Nachname;Alter;Email\nMax;Mustermann;30;max@example.com\nAnna;Schmidt;25;anna@example.com";

        // Act
        var result = CsvJsonConverter.FromCsv<Person>(csv, ';');

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Max", result[0].Vorname);
        Assert.Equal("Mustermann", result[0].Nachname);
        Assert.Equal(30, result[0].Alter);
        Assert.Equal("max@example.com", result[0].Email);
    }

    [Fact]
    public void FromCsv_WithEmptyString_ReturnsEmptyList()
    {
        // Act
        var result = CsvJsonConverter.FromCsv<Person>(string.Empty);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FromCsv_WithOnlyHeader_ReturnsEmptyList()
    {
        // Arrange
        string csv = "Vorname;Nachname;Alter;Email";

        // Act
        var result = CsvJsonConverter.FromCsv<Person>(csv);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(';')]
    [InlineData(',')]
    public void FromCsv_WithDifferentDelimiters_ParsesCorrectly(char delimiter)
    {
        // Arrange
        string csv = $"Vorname{delimiter}Nachname{delimiter}Alter{delimiter}Email\nMax{delimiter}Mustermann{delimiter}30{delimiter}max@example.com";

        // Act
        var result = CsvJsonConverter.FromCsv<Person>(csv, delimiter);

        // Assert
        Assert.Single(result);
        Assert.Equal("Max", result[0].Vorname);
    }

    #endregion

    #region Roundtrip Tests

    [Fact]
    public void Roundtrip_ToCsvAndBack_PreservesData()
    {
        // Arrange
        var original = new List<Person>
        {
            new("Max", "Mustermann", 30, "max@example.com"),
            new("Anna", "Schmidt", 25, "anna@example.com"),
            new("Peter", "Mueller", 45, "peter@example.com")
        };

        // Act
        string csv = CsvJsonConverter.ToCsv(original, ';');
        var restored = CsvJsonConverter.FromCsv<Person>(csv, ';');

        // Assert
        Assert.Equal(original.Count, restored.Count);
        for (int i = 0; i < original.Count; i++)
        {
            Assert.Equal(original[i], restored[i]);
        }
    }

    [Fact]
    public void FromCsv_WithCaseInsensitiveHeaders_ParsesCorrectly()
    {
        // Arrange - Header in Kleinbuchstaben
        string csv = "vorname;nachname;alter;email\nMax;Mustermann;30;max@example.com";

        // Act
        var result = CsvJsonConverter.FromCsv<Person>(csv, ';');

        // Assert
        Assert.Single(result);
        Assert.Equal("Max", result[0].Vorname);
    }

    #endregion
}
