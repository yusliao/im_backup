using System;
using System.Windows;
using System.Windows.Data;
using IMModels;

namespace IMClient.Converter
{
    class PhoneNumberToView : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string phone = string.Empty;
            if (value is PersonCardModel personCardModel)
            {
                if (personCardModel.ID == ViewModels.AppData.Current.LoginUser.ID)
                {
                    phone = personCardModel.PhoneNumber;
                }
                else
                {
                    if (!string.IsNullOrEmpty(personCardModel.PhoneNumber) && personCardModel.PhoneNumber.Length == 11)
                    {
                        phone = personCardModel.PhoneNumber.Remove(3, 4);
                        phone = phone.Insert(3, "****");
                    }
                }
            }
            else if (value is UserModel user)
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
            else if(value is string)
            {
                var phoneNumber = value.ToString();
                if (phoneNumber.Length == 11)
                {
                    phone = phoneNumber.Remove(3, 4);
                    phone = phone.Insert(3, "****");
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
