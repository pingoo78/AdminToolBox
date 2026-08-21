using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace AdminToolBox.Controls;

/// <summary>
/// Enchaîne ses enfants horizontalement et passe à la ligne quand ils dépassent la largeur
/// disponible, chacun conservant sa hauteur naturelle. Contrairement à <see cref="ItemsWrapGrid"/>,
/// qui impose une hauteur de cellule uniforme à tous les éléments (gâchant l'espace sous les
/// éléments courts quand un autre est plus grand), chaque ligne ne prend que la hauteur de son
/// élément le plus grand.
/// </summary>
public sealed class WrapPanel : Panel
{
    public double HorizontalSpacing { get; set; }
    public double VerticalSpacing { get; set; }

    protected override Size MeasureOverride(Size availableSize)
    {
        double rowWidth = 0, rowHeight = 0, totalWidth = 0, totalHeight = 0;

        foreach (var child in Children)
        {
            child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
            var size = child.DesiredSize;

            if (rowWidth > 0 && rowWidth + size.Width > availableSize.Width)
            {
                totalWidth = Math.Max(totalWidth, rowWidth - HorizontalSpacing);
                totalHeight += rowHeight + VerticalSpacing;
                rowWidth = 0;
                rowHeight = 0;
            }

            rowWidth += size.Width + HorizontalSpacing;
            rowHeight = Math.Max(rowHeight, size.Height);
        }

        totalWidth = Math.Max(totalWidth, rowWidth - HorizontalSpacing);
        totalHeight += rowHeight;

        return new Size(Math.Max(0, totalWidth), Math.Max(0, totalHeight));
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0, y = 0, rowHeight = 0;

        foreach (var child in Children)
        {
            var size = child.DesiredSize;

            if (x > 0 && x + size.Width > finalSize.Width)
            {
                y += rowHeight + VerticalSpacing;
                x = 0;
                rowHeight = 0;
            }

            child.Arrange(new Rect(x, y, size.Width, size.Height));
            x += size.Width + HorizontalSpacing;
            rowHeight = Math.Max(rowHeight, size.Height);
        }

        return finalSize;
    }
}
