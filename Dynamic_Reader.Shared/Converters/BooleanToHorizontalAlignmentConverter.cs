using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Dynamic_Reader.Converters
{
    public class BooleanToHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
            {
                return HorizontalAlignment.Left;
            }

            return HorizontalAlignment.Center;
        }


        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
