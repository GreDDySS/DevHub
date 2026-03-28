using System.Globalization;
using System.Windows.Data;

namespace DevHub.Presentation.Converters;

public class ViewModeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            ViewMode.List => "▦ Tiles",
            ViewMode.Tiles => "☰ List",
            ViewMode.Folders => "☰ List",
            _ => "☰ List"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
