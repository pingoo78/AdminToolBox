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
