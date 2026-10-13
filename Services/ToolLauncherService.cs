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
