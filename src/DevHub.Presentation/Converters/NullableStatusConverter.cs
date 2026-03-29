using System.Globalization;
using System.Windows.Data;

namespace DevHub.Presentation.Converters;

public class NullableStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DevHub.Domain.Enums.ProjectStatus status)
            return status.ToString();
        return "All";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
