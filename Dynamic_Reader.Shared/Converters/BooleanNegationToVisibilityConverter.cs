using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Dynamic_Reader.Converters
{
    /// <summary>
    ///     Value converter that translates true to <see cref="Visibility.Visible" /> and false to
    ///     <see cref="Visibility.Collapsed" />.
    /// </summary>
    public sealed class BooleanNegationToVisibilityConverter : IValueConverter
    {
        public object Convert( object value,  Type targetType,  object parameter,
             string language)
        {
            return (value is bool && (bool) value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack( object value,  Type targetType,  object parameter,
             string language)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (targetType == null) throw new ArgumentNullException("targetType");
            if (parameter == null) throw new ArgumentNullException("parameter");
            if (language == null) throw new ArgumentNullException("language");

            return value is Visibility && (Visibility) value == Visibility.Collapsed;
        }
    }
}