using IMClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace IMClient.Converter
{
    class BoolToVisibilityConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && (bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class BoolConvertVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if ((bool)value)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class BoolConvertVisibilityEx : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if ((bool)value)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class ReverseBoolConvertVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if ((bool)value)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 多路数据绑定转换器，使用如
    /// <CommanParameter>
    ///     <MultiBinding>
    ///         <Binding/>
    ///         <Binding/>
    ///     </MultiBinding>
    /// </CommanParameter>
    /// </summary>
    public class MultiParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targettype, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values != null)
            {
                string para = (string)parameter;
                if (!string.IsNullOrEmpty(para))
                {
                    Dictionary<string, object> paraDic = new Dictionary<string, object>();
                    int i = 1;
                    foreach (object obj in values)
                    {
                        paraDic.Add("context" + i, obj);
                        i++;
                    }
                    MatchCollection mc = Regex.Matches(para, @"[^\|]+");
                    foreach (Match m1 in mc)
                    {
                        Match m2 = Regex.Match(m1.Value, @"(?<name>[^:]+):(?<val>[^:]+):(?<type>[^\|]+)");
                        if (m2.Success)
                        {
                            switch (m2.Groups["type"].Value)
                            {
                                case "i":
                                    paraDic[m2.Groups["name"].Value] = int.Parse(m2.Groups["val"].Value);
                                    break;
                                case "b":
                                    paraDic[m2.Groups["name"].Value] = bool.Parse(m2.Groups["val"].Value);
                                    break;
                                default:
                                    paraDic[m2.Groups["name"].Value] = m2.Groups["val"].Value;
                                    break;
                            }
                        }
                    }
                    return paraDic;
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 方法转换器，传数据控件都行
    /// </summary>
    public class MethodParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targettype, object parameter, System.Globalization.CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 单路数据转换器
    /// </summary>
    public class ParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string para = (string)parameter;
            if (!string.IsNullOrEmpty(para))
            {
                Dictionary<string, object> paraDic = new Dictionary<string, object>() { { "context", value } };
                MatchCollection mc = Regex.Matches(para, @"[^\|]+");
                foreach (Match m1 in mc)
                {
                    Match m2 = Regex.Match(m1.Value, @"(?<name>[^:]+):(?<val>[^:]+):(?<type>[^\|]+)");
                    if (m2.Success)
                    {
                        switch (m2.Groups["type"].Value)
                        {
                            case "i":
                                paraDic[m2.Groups["name"].Value] = int.Parse(m2.Groups["val"].Value);
                                break;
                            case "b":
                                paraDic[m2.Groups["name"].Value] = bool.Parse(m2.Groups["val"].Value);
                                break;
                            default:
                                paraDic[m2.Groups["name"].Value] = m2.Groups["val"].Value;
                                break;
                        }
                    }
                }
                return paraDic;
            }
            return null;
        }
        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringEmptyConvereter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string para = (string)value;
            if (!string.IsNullOrEmpty(para))
                return System.Windows.Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class JoinGroupCheckConvereter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            GroupViewModel groupViewModel = value as GroupViewModel;
            if(groupViewModel!=null)
            {
                if (groupViewModel.IsCreator || groupViewModel.IsAdmin)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
