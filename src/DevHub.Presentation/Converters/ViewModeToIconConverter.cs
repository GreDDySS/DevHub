using System.Globalization;
using System.Windows.Data;

namespace DevHub.Presentation.Converters;

public enum ViewMode
{
    Tiles,
    List
}

public class ViewModeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is ViewMode.List ? "▦ Tiles" : "☰ List";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
