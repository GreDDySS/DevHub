using System.Globalization;
using System.Windows.Data;

namespace DevHub.Presentation.Converters;

public class ProjectStatusColorConverter : IValueConverter
{
    private static readonly System.Windows.Media.Brush ActiveBrush = CreateBrush(76, 175, 80);
    private static readonly System.Windows.Media.Brush CompletedBrush = CreateBrush(33, 150, 243);
    private static readonly System.Windows.Media.Brush PausedBrush = CreateBrush(255, 152, 0);
    private static readonly System.Windows.Media.Brush ArchivedBrush = CreateBrush(158, 158, 158);
    private static readonly System.Windows.Media.Brush DefaultBrush = System.Windows.Media.Brushes.Black;

    private static System.Windows.Media.SolidColorBrush CreateBrush(byte r, byte g, byte b)
    {
        var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is DevHub.Domain.Enums.ProjectStatus status ? status switch
        {
            DevHub.Domain.Enums.ProjectStatus.Active => ActiveBrush,
            DevHub.Domain.Enums.ProjectStatus.Completed => CompletedBrush,
            DevHub.Domain.Enums.ProjectStatus.Paused => PausedBrush,
            DevHub.Domain.Enums.ProjectStatus.Archived => ArchivedBrush,
            _ => DefaultBrush
        } : DefaultBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
