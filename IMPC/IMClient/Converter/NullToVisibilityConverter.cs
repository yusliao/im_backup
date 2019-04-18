using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace IMClient.Converter
{
    public class NullToVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //if (value == null) return 0;
            //if (string.IsNullOrEmpty(value.ToString())) return 0;
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Visibility.Collapsed;
            else
            {
                return Visibility.Visible;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class ReverseNullToVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //if (value == null) return 0;
            //if (string.IsNullOrEmpty(value.ToString())) return 0;
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Visibility.Visible;
            else
            {
                return Visibility.Collapsed;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}
