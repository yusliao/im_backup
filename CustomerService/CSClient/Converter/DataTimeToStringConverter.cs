using System;
using System.Windows;
using System.Windows.Data;

namespace CSClient.Converter
{
    class DataTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime dt;
            if (value is DateTime)
            {
                dt = (DateTime)value;
                if(dt == DateTime.MinValue)
                {
                    return string.Empty;
                }
            }
            else 
            {
                DateTime.TryParse(string.Format("{0}", value), out dt);                
            }

            string result = string.Empty;
            if (dt < DateTime.Now.Date)
            {
                result = dt.ToString("MM/dd");
            }
            else
            {
                result = dt.ToString("HH:mm");
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class SendTimeToViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime dt;
            if (value is DateTime)
            {
                dt = (DateTime)value;
            }
            else
            {
                DateTime.TryParse(string.Format("{0}", value), out dt);
            }

            int date = (DateTime.Now.Date - dt.Date).Days;

            string result = string.Empty;
            if (date == 0) //今天
            {
                result = string.Format("{0:HH:mm:ss}", dt);
            }
            else if (date == 1) //昨天
            {
                result = string.Format("昨天 {0:HH:mm:ss}", dt);
            }
            else if(DateTime.Now.Year == dt.Year)
            {
                result = string.Format("{0:MM/dd HH:mm:ss}", dt);
            }
            else
            {
                result = string.Format("{0:yyyy/MM/dd HH:mm:ss}", dt);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
