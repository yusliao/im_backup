using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace IMClient.Converter
{
    public class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = (string)value;
            if (parameter != null)
            {
                result = GetSubString(result, System.Convert.ToInt32(parameter));
            }
            return result;
        }

        private string GetSubString(string str, int length)
        {
            if (string.IsNullOrEmpty(str)) return "";

            str = str.Split('\\').LastOrDefault();
            int suffixCharCount = 6;//长度超过时，末尾保留的字符数，如：1231414...12.txt

            //全部所占字节
            int fullLenght = getStringLength(str);

            if (fullLenght < suffixCharCount + length + 3)
            {
                return str;
            }

            string prefix = str.Substring(0, length);
            double perLength = getStringLength(prefix);
            int canLength = length;
            while (perLength > length)
            {
                canLength -= 1;
                prefix = str.Substring(0, canLength);
                perLength = getStringLength(prefix);
            }

            string suffix = str.Substring(str.Length - 6, suffixCharCount);

            return string.Format("{0}…{1}", prefix, suffix);
        }

        /// <summary>
        /// 获取中英文混排字符串的实际长度(字节数)
        /// </summary>
        /// <param name="str">要获取长度的字符串</param>
        /// <returns>字符串的实际长度值（字节数）</returns>
        public int getStringLength(string str)
        {
            if (str.Equals(string.Empty))
                return 0;
            int strlen = 0;
            ASCIIEncoding strData = new ASCIIEncoding();
            //将字符串转换为ASCII编码的字节数字
            byte[] strBytes = strData.GetBytes(str);
            for (int i = 0; i <= strBytes.Length - 1; i++)
            {
                if (strBytes[i] == 63)  //中文都将编码为ASCII编码63,即"?"号
                    strlen++;
                strlen++;
            }
            return strlen;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = (string)value;
            return result;
        }
    }

    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetResult(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object GetResult(object value)
        {

           return  Helper.FileHelper.FileSizeToString((long)value); 
        }
    }
}
