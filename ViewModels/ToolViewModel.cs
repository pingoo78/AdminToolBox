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
using AdminToolBox.Models;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AdminToolBox.ViewModels;

public sealed class ToolViewModel(ToolItem tool, bool isAvailable) : INotifyPropertyChanged
{
    private const int FavoriteGlyphCodePoint = 0xE735;
    private const int NotFavoriteGlyphCodePoint = 0xE734;

    private BitmapImage? _icon;

    public ToolItem Tool { get; } = tool;
    public string Name => Tool.Name;
    public bool IsAvailable { get; } = isAvailable;
    public double Opacity => IsAvailable ? 1.0 : 0.4;

    public string Description
    {
        get
        {
            var text = $"{Tool.Description}\nCommande : {Tool.CommandLine}";
            return IsAvailable ? text : $"{text}\n(indisponible sur ce poste)";
        }
    }

    public BitmapImage? Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
        }
    }

    private bool _isFavorite;

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            _isFavorite = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFavorite)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FavoriteGlyph)));
        }
    }

    /// <summary>Glyphe Segoe Fluent Icons : étoile pleine si favori, contour sinon.</summary>
    public string FavoriteGlyph => char.ConvertFromUtf32(IsFavorite ? FavoriteGlyphCodePoint : NotFavoriteGlyphCodePoint);

    public event PropertyChangedEventHandler? PropertyChanged;
}
