using System.Globalization;
using System.Windows.Data;

namespace DevHub.Presentation.Converters;

public class BoolToHideConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? "Unhide" : "Hide";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
