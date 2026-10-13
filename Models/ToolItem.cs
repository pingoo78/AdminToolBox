namespace AdminToolBox.Models;

/// <summary>
/// Décrit un raccourci vers un outil Windows (exécutable, console MMC, applet
/// du panneau de configuration ou protocole) lançable depuis la toolbox.
/// </summary>
public sealed class ToolItem
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required ToolCategory Category { get; init; }

    /// <summary>Fichier ou protocole passé à ShellExecute (ex. "eventvwr.msc", "control.exe", "windowsdefender:").</summary>
    public required string FileName { get; init; }

    public string Arguments { get; init; } = string.Empty;

    /// <summary>
    /// Fichier dont on extrait l'icône affichée, s'il diffère de <see cref="FileName"/>
    /// (ex. rundll32.exe qui utilise l'icône de sysdm.cpl).
    /// </summary>
    public string? IconFileName { get; init; }

    /// <summary>
    /// Nom de famille de paquet MSIX (ex. "Microsoft.WindowsTerminal_8wekyb3d8bbwe") dont on
    /// extrait <see cref="IconLookupFile"/> depuis le dossier d'installation réel, quand le
    /// binaire lancé n'est qu'un stub App Execution Alias sans icône propre.
    /// </summary>
    public string? IconPackageFamilyName { get; init; }

    /// <summary>
    /// Chemin absolu vers une image déjà prête (PNG, etc.) à utiliser telle quelle comme icône,
    /// prioritaire sur toute extraction. Utile quand l'icône embarquée dans l'exécutable n'est pas
    /// la bonne (ex. Paramètres Windows, dont l'.exe ne contient qu'une icône plate obsolète).
    /// </summary>
    public string? IconImagePath { get; init; }

    /// <summary>Prérequis affiché quand on clique sur l'outil absent du poste (par défaut : message RSAT).</summary>
    public string? UnavailableHint { get; init; }

    public string IconLookupFile => IconFileName ?? FileName;

    /// <summary>Ligne de commande réellement passée à ShellExecute, affichée dans l'infobulle.</summary>
    public string CommandLine => string.IsNullOrEmpty(Arguments) ? FileName : $"{FileName} {Arguments}";
}
