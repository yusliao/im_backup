using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using CSClient.Views.Controls;
using IMModels;

namespace CSClient.Converter
{
    public class MessageToViewConverter : IValueConverter
    { 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MessageModel msg = value as MessageModel;

            if (msg != null)
            {
                switch (msg.MsgType)
                {
                    case MessageType.img:
                        CSClient.Helper.MessageHelper.LoadImgContent(msg);
                        return msg;
                    case MessageType.file:
                        //msg.FileImg = IMAssets.ImageDeal.DefaultDownloadImg;
                        //CSClient.Helper.MessageHelper.LoadFileContent(msg);
                        if (!msg.IsSending)
                        {
                            string fullName = msg.ResourceModel.FullName;
                            if (!File.Exists(fullName))
                            {
                                fullName = Path.Combine(SDKClient.SDKClient.Instance.property.CurrentAccount.filePath, msg.ResourceModel.FileName);
                            }
                            msg.Content = fullName; 
                        } 
                        break;
                    default:
                        CSClient.Helper.MessageHelper.ContentInlines = null;
                        return  CSClient.Helper.MessageHelper.GetRichContent(msg.Content, msg.IsMine); 
                        ////var inlines= CSClient.Helper.MessageHelper.GetRichContent(msg.Content);

                        ////if (inlines == null)
                        ////{
                        ////    return new TextBox() { Text = msg.Content, Style = parameter as Style, TextWrapping = TextWrapping.Wrap };
                        ////}
                        ////else
                        ////{
                        ////    RichTextBox rict = new RichTextBox() { Style = parameter as Style, AcceptsReturn = true };

                        ////    Paragraph parph = new Paragraph();
                        ////    parph.Inlines.AddRange(inlines); 
                        ////    rict.Document.Blocks.Add(parph); 

                        ////    return rict;
                        ////}

                        //RichTextBox rict = new RichTextBox() { Style = parameter as Style, };

                        //CSClient.Helper.MessageHelper.AppendContentToRichTextBox(rict, msg.Content);
                        //return rict;
                }
            }
            

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        } 
     
    } 
}
