using System.Reflection;

namespace AdminToolBox.Services;

/// <summary>Métadonnées de build de l'application (version, date de compilation).</summary>
public static class AppInfoService
{
    public static string GetVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version is null ? "?" : $"{version.Major}.{version.Minor}.{version.Build}";
    }

    /// <summary>
    /// Les builds .NET étant déterministes, l'horodatage du PE n'est plus fiable : on se base
    /// à la place sur la date de dernière écriture de l'assembly compilé.
    /// </summary>
    public static DateTime GetBuildDate() => File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location);
}
