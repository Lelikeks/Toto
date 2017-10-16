using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Toto
{
	class CheckedValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null || value.GetType() != typeof(bool))
			{
				return null;
			}
			return (bool) value ? Brushes.Aqua : Brushes.Transparent;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
