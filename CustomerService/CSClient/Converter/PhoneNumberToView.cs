using System;
using System.Windows;
using System.Windows.Data;
using IMModels;

namespace CSClient.Converter
{
    class PhoneNumberToView : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string phone = string.Empty;
            if(value is UserModel user)
            {
                if (user.ID == ViewModels.AppData.Current.LoginUser.ID)
                {
                    phone = user.PhoneNumber;
                }
                else
                {
                    if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumber.Length == 11)
                    {
                        phone = user.PhoneNumber.Remove(3, 4);
                        phone = phone.Insert(3, "****");
                    }                    
                }  
            }
           
            return phone;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

 
}
