using CSClient.ViewModels;
using IMModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CSClient.Converter
{
    public class ModifyGroupNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is ChatModel))
            {
                return Visibility.Collapsed;
            }

            ChatModel chatModel = value as ChatModel;

            if (chatModel.Chat is UserModel)
            {
                return Visibility.Collapsed;
            }
            else 
            {
                GroupModel model = (GroupModel)chatModel.Chat;
                if (model.OwnerID == AppData.MainMV.LoginUser.ID)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
