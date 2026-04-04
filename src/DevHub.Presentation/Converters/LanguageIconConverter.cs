using System.Globalization;
using System.Windows.Data;
using DevHub.Domain.Enums;

namespace DevHub.Presentation.Converters;

public class LanguageIconConverter : IValueConverter
{
    private static readonly Dictionary<ProgrammingLanguage, string> Icons = new()
    {
        [ProgrammingLanguage.CSharp] = "\U0001F537",
        [ProgrammingLanguage.Python] = "\U0001F40D",
        [ProgrammingLanguage.Rust] = "\U0001F980",
        [ProgrammingLanguage.JavaScript] = "\U0001F7E8",
        [ProgrammingLanguage.TypeScript] = "\U0001F535",
        [ProgrammingLanguage.Go] = "\U0001F439",
        [ProgrammingLanguage.Java] = "\u2615",
        [ProgrammingLanguage.Cpp] = "\u2699",
        [ProgrammingLanguage.Other] = "\U0001F4C4"
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is ProgrammingLanguage lang && Icons.TryGetValue(lang, out var icon)
            ? icon
            : "\U0001F4C4";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
