using System.Globalization;
using System.Windows.Data;
using DevHub.Domain.Enums;

namespace DevHub.Presentation.Converters;

public class LinkTypeToColorConverter : IValueConverter
{
    private static readonly System.Windows.Media.Brush YouTubeBrush = CreateBrush(255, 0, 0);
    private static readonly System.Windows.Media.Brush RepositoryBrush = CreateBrush(36, 41, 46);
    private static readonly System.Windows.Media.Brush ArticleBrush = CreateBrush(33, 150, 243);
    private static readonly System.Windows.Media.Brush DocumentationBrush = CreateBrush(76, 175, 80);
    private static readonly System.Windows.Media.Brush DefaultBrush = System.Windows.Media.Brushes.Gray;

    private static System.Windows.Media.SolidColorBrush CreateBrush(byte r, byte g, byte b)
    {
        var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not LinkType type ? DefaultBrush : type switch
        {
            LinkType.YouTube => YouTubeBrush,
            LinkType.Repository => RepositoryBrush,
            LinkType.Article => ArticleBrush,
            LinkType.Documentation => DocumentationBrush,
            _ => DefaultBrush
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
