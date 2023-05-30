using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Dynamic_Reader.Converters
{
    public class VisibilityToBooleanConverter : IValueConverter
    {
        public object Convert( object value,  Type targetType,  object parameter,
             string language)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (targetType == null) throw new ArgumentNullException("targetType");
            if (parameter == null) throw new ArgumentNullException("parameter");
            if (language == null) throw new ArgumentNullException("language");
            var v = (Visibility) value;
            switch (v)
            {
                case Visibility.Collapsed:
                    return false;
                case Visibility.Visible:
                    return true;
                default:
                    return null;
            }
        }

        public object ConvertBack( object value,  Type targetType,  object parameter,
             string language)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (targetType == null) throw new ArgumentNullException("targetType");
            if (parameter == null) throw new ArgumentNullException("parameter");
            if (language == null) throw new ArgumentNullException("language");
            throw new NotImplementedException();
        }
    }
}