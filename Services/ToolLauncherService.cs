using System.ComponentModel;
using System.Diagnostics;
using AdminToolBox.Models;

namespace AdminToolBox.Services;

public sealed class ToolLaunchException(ToolItem tool, Exception inner)
    : Exception($"Impossible de lancer « {tool.Name} ». L'outil n'est peut-être pas installé sur ce poste (RSAT manquant ?).", inner)
{
    public ToolItem Tool { get; } = tool;
}

/// <summary>Lance les outils du catalogue en héritant du jeton (élevé) du process courant.</summary>
public static class ToolLauncherService
{
    public static void Launch(ToolItem tool)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = tool.FileName,
            Arguments = tool.Arguments,
            UseShellExecute = true,
        };

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception ex) when (ex is Win32Exception or FileNotFoundException)
        {
            throw new ToolLaunchException(tool, ex);
        }
    }
}
