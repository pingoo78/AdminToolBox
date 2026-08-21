namespace AdminToolBox.Models;

/// <summary>Une entrée du panneau d'informations système (pictogramme, libellé, valeur).</summary>
public sealed record InfoItem(string Glyph, string Label, string Value);
