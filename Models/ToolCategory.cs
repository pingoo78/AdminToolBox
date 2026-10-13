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

namespace AdminToolBox.Models;

public enum ToolCategory
{
    Systeme,
    Reseau,
    Securite,
    ActiveDirectory,
    Consoles
}

public static class ToolCategoryExtensions
{
    public static string ToDisplayName(this ToolCategory category) => category switch
    {
        ToolCategory.Systeme => "Système",
        ToolCategory.Reseau => "Réseau",
        ToolCategory.Securite => "Sécurité",
        ToolCategory.ActiveDirectory => "Active Directory",
        ToolCategory.Consoles => "Consoles",
        _ => category.ToString()
    };
}
