using System.Globalization;
using System.Windows.Data;
using DevHub.Domain.Enums;

namespace DevHub.Presentation.Converters;

public class LinkTypeToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LinkType type) return System.Windows.Media.Brushes.Gray;

        return type switch
        {
            LinkType.YouTube => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 0, 0)),
            LinkType.Repository => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(36, 41, 46)),
            LinkType.Article => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(33, 150, 243)),
            LinkType.Documentation => new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(76, 175, 80)),
            _ => System.Windows.Media.Brushes.Gray
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
