// This file is part of AdminToolBox.
// Copyright (C) 2026 Alexandre ARMENIAN
//
// AdminToolBox is free software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation, either version 3
// of the License, or (at your option) any later version.
//
// AdminToolBox is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program.
// If not, see <https://www.gnu.org/licenses/>.

using System.Text.Json;

namespace AdminToolBox.Services;

public sealed class AppSettings
{
    public string Theme { get; set; } = "Default";
    public List<string> Favorites { get; set; } = [];
    public bool InfoPanelsCollapsed { get; set; }
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
            var settings = File.Exists(FilePath)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new AppSettings()
                : new AppSettings();

            // Le fichier est modifiable par l'utilisateur : on neutralise les valeurs null/hors
            // contrat plutôt que de planter au démarrage.
            settings.Favorites ??= [];
            settings.Theme = settings.Theme is "Light" or "Dark" ? settings.Theme : "Default";
            return settings;
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
