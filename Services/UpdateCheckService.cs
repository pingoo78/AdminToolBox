using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AdminToolBox.Services;

public sealed record UpdateCheckResult(bool Success, bool UpdateAvailable, string? LatestVersion);

/// <summary>Interroge la dernière release GitHub du dépôt du projet pour signaler une mise à jour disponible.</summary>
public static class UpdateCheckService
{
    private const string ReleasesApiUrl = "https://api.github.com/repos/pingoo78/AdminToolBox/releases/latest";

    /// <remarks>
    /// Le dépôt est actuellement privé : l'appel anonyme échouera (404) tant qu'il ne sera pas
    /// public. On l'échoue silencieusement dans ce cas plutôt que d'alerter l'utilisateur.
    /// </remarks>
    public static async Task<UpdateCheckResult> CheckForUpdateAsync(string currentVersion)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AdminToolBox", currentVersion));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using var response = await client.GetAsync(ReleasesApiUrl);
            if (!response.IsSuccessStatusCode)
            {
                return new UpdateCheckResult(false, false, null);
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            var tagName = document.RootElement.GetProperty("tag_name").GetString();
            var latest = tagName?.TrimStart('v', 'V');

            if (latest is null || !Version.TryParse(latest, out var latestVersion) || !Version.TryParse(currentVersion, out var current))
            {
                return new UpdateCheckResult(true, false, latest);
            }

            return new UpdateCheckResult(true, latestVersion > current, latest);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new UpdateCheckResult(false, false, null);
        }
    }
}
