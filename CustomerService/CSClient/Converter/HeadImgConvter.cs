using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CSClient.Converter
{
    class HeadImgConvter : IValueConverter
    {
        static Dictionary<string, ImageSource> _headImgs = new Dictionary<string, ImageSource>();
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //App.Current.Dispatcher.Invoke(new Action(() =>
            //{
            //    Console.WriteLine($"AAAAAAAAAAAAAAAAAphoto = {value}");
            //}));
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
                        ImageSource source = Helper.WindowsThumbnailProvider.GetFileThumbnail(path, size, size, Helper.ThumbnailOptions.ThumbnailOnly);
                         
                        _headImgs.Add(path, source);
                        result = source;
                    } 
                }
                else if(string.IsNullOrEmpty(path))
                {
                    result = IMAssets.ImageDeal.DefaultHeadImage;
                }
            }
            else
            {
                result = IMAssets.ImageDeal.DefaultHeadImage;
            }

             

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    } 
}
