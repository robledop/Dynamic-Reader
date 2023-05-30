using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Dynamic_Reader.Converters
{
	/// <summary>
	///     Value converter that translates true to false and vice versa.
	/// </summary>
	public sealed class BookTitleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter,
			 string language)
		{
			var cover = value as string;
			if (cover == "/Assets/book.jpg")
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			 string language)
		{
			return !(value is bool && (bool)value);
		}
	}
}