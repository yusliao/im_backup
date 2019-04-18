using IMClient.ViewModels;
using IMModels;
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
                var groupVM = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(i => i.ID == model.ID);
                if (groupVM != null)
                {
                    if (groupVM.IsAdmin || groupVM.IsCreator)
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
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
