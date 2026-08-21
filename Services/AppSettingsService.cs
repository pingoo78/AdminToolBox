using System.Text.Json;

namespace AdminToolBox.Services;

public sealed class AppSettings
{
    public string Theme { get; set; } = "Default";
    public List<string> Favorites { get; set; } = [];
}

/// <summary>
/// Persistance simple en JSON sous %LocalAppData% : l'application n'étant pas empaquetée (MSIX),
/// l'API ApplicationData (nécessite une identité de paquet) n'est pas disponible ici.
/// </summary>
public static class AppSettingsService
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AdminToolBox",
        "settings.json");

    public static AppSettings Load()
    {
        try
        {
            return File.Exists(FilePath)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings()
                : new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Persistance best-effort : une écriture ratée ne doit pas planter l'application.
        }
    }
}
