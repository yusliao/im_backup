using IMClient.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IMClient.Converter
{
    class HeadImgConvter : IValueConverter
    {
        static Dictionary<string, BitmapSource> _headImgs = new Dictionary<string, BitmapSource>();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object result = value;
            if (value != null && value is string path)
            {
                if (path!=IMAssets.ImageDeal.DefaultHeadImage && System.IO.File.Exists(path))
                 {
                    if (_headImgs.ContainsKey(path))
                    {
                        result= _headImgs[path];
                    }
                    else
                    {
                        int size = 60;
                        if (parameter != null)
                        {
                            int s;
                            if (int.TryParse(parameter.ToString(), out s) && s > 0)
                            {
                                size = s;
                            }
                        }
                        var source = Helper.WindowsThumbnailProvider.GetFileThumbnail(path, size, size, Helper.ThumbnailOptions.ThumbnailOnly);
                        _headImgs.Add(path, source);
                        result = source;
                    } 
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

       
    }
     
    
}
