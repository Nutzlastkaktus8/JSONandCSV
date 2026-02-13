namespace JSONandCSV;

/// <summary>
/// Datenmodell für eine Person.
/// </summary>
public class Person(string vorname, string nachname, int alter, string email)
{
    public string Vorname { get; } = vorname;
    public string Nachname { get; } = nachname;
    public int Alter { get; } = alter;
    public string Email { get; } = email;
}
