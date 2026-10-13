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

using System.Runtime.InteropServices;
using AdminToolBox.Models;
using Microsoft.Win32;
using Windows.Management.Deployment;

namespace AdminToolBox.Services;

/// <summary>
/// Vérifie si le binaire/console d'un <see cref="ToolItem"/> est réellement présent sur le
/// poste (utile pour les consoles RSAT/Active Directory absentes des postes non-serveur).
/// </summary>
public static class ToolAvailabilityService
{
    public static bool IsAvailable(ToolItem tool) => ResolveFullPath(tool.IconLookupFile) is not null || IsProtocol(tool.FileName);

    /// <summary>Résout le chemin complet d'un exécutable/console système, ou null s'il est introuvable.</summary>
    public static string? ResolveFullPath(string fileName)
    {
        if (IsProtocol(fileName))
        {
            var scheme = fileName.TrimEnd(':');
            return Registry.ClassesRoot.OpenSubKey(scheme) is not null ? fileName : null;
        }

        var candidateDirs = new List<string>
        {
            Environment.SystemDirectory,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64"),
        };

        var pathVar = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathVar))
        {
            candidateDirs.AddRange(pathVar.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries));
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        candidateDirs.Add(Path.Combine(localAppData, "Microsoft", "WindowsApps"));

        foreach (var dir in candidateDirs)
        {
            try
            {
                var candidate = Path.Combine(dir, fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            catch (ArgumentException)
            {
                // Répertoire de PATH mal formé : on ignore et on continue.
            }
        }

        return null;
    }

    /// <summary>
    /// Résout le chemin complet d'un fichier à l'intérieur du dossier d'installation réel d'un
    /// paquet MSIX (utile pour extraire l'icône d'un exécutable dont l'alias App Execution Alias
    /// n'a pas d'icône propre, ex. Windows Terminal).
    /// </summary>
    public static string? ResolvePackagedFullPath(string packageFamilyName, string fileName)
    {
        try
        {
            var package = new PackageManager().FindPackagesForUser(string.Empty, packageFamilyName).FirstOrDefault();
            if (package is null)
            {
                return null;
            }

            var candidate = Path.Combine(package.InstalledLocation.Path, fileName);
            return File.Exists(candidate) ? candidate : null;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException or COMException)
        {
            return null;
        }
    }

    private static bool IsProtocol(string fileName) => fileName.EndsWith(':');
}
