using System.Globalization;
using System.Windows.Data;
using WpfMedia = System.Windows.Media;

namespace DevHub.Presentation.Converters;

public class ProjectStatusColorConverter : IValueConverter
{
    private static readonly WpfMedia.SolidColorBrush ActiveBrush = CreateBrush(34, 124, 44);
    private static readonly WpfMedia.SolidColorBrush CompletedBrush = CreateBrush(25, 130, 216);
    private static readonly WpfMedia.SolidColorBrush PausedBrush = CreateBrush(180, 120, 0);
    private static readonly WpfMedia.SolidColorBrush ArchivedBrush = CreateBrush(100, 100, 100);
    private static readonly WpfMedia.SolidColorBrush DefaultBrush = CreateBrush(128, 128, 128);

    private static WpfMedia.SolidColorBrush CreateBrush(byte r, byte g, byte b)
    {
        var brush = new WpfMedia.SolidColorBrush(WpfMedia.Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Domain.Enums.ProjectStatus status ? status switch
        {
            Domain.Enums.ProjectStatus.Active => ActiveBrush,
            Domain.Enums.ProjectStatus.Completed => CompletedBrush,
            Domain.Enums.ProjectStatus.Paused => PausedBrush,
            Domain.Enums.ProjectStatus.Archived => ArchivedBrush,
            _ => DefaultBrush
        } : DefaultBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class StatusBackgroundConverter : IValueConverter
{
    private static readonly WpfMedia.SolidColorBrush ActiveBg = CreateBrush(34, 124, 44, 30);
    private static readonly WpfMedia.SolidColorBrush CompletedBg = CreateBrush(25, 130, 216, 30);
    private static readonly WpfMedia.SolidColorBrush PausedBg = CreateBrush(180, 120, 0, 30);
    private static readonly WpfMedia.SolidColorBrush ArchivedBg = CreateBrush(100, 100, 100, 30);
    private static readonly WpfMedia.SolidColorBrush DefaultBg = CreateBrush(128, 128, 128, 30);

    private static WpfMedia.SolidColorBrush CreateBrush(byte r, byte g, byte b, byte a)
    {
        var brush = new WpfMedia.SolidColorBrush(WpfMedia.Color.FromArgb(a, r, g, b));
        brush.Freeze();
        return brush;
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Domain.Enums.ProjectStatus status ? status switch
        {
            Domain.Enums.ProjectStatus.Active => ActiveBg,
            Domain.Enums.ProjectStatus.Completed => CompletedBg,
            Domain.Enums.ProjectStatus.Paused => PausedBg,
            Domain.Enums.ProjectStatus.Archived => ArchivedBg,
            _ => DefaultBg
        } : DefaultBg;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}