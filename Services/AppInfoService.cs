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
