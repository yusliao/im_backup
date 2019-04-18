using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace IMClient.Helper
{
    public class MaxByteAttachedProperty : DependencyObject
    {
        public enum Encode
        {
            Default,
            ASCII,
            UTF8,
            UTF32,
            UTF7,
            BigEndianUnicode,
            Unicode
        }


        private static string GetPreText(DependencyObject obj)
        {
            return (string)obj.GetValue(PreTextProperty);
        }

        private static void SetPreText(DependencyObject obj, string value)
        {
            obj.SetValue(PreTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for PreText. This enables animation, styling, binding, etc...
        private static readonly DependencyProperty PreTextProperty =
       DependencyProperty.RegisterAttached("PreText", typeof(string), typeof(MaxByteAttachedProperty), new PropertyMetadata(""));


        public static int GetMaxByteLength(DependencyObject obj)
        {
            return (int)obj.GetValue(MaxByteLengthProperty);
        }

        public static void SetMaxByteLength(DependencyObject obj, int value)
        {
            obj.SetValue(MaxByteLengthProperty, value);
        }

        // Using a DependencyProperty as the backing store for MaxByteLength. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxByteLengthProperty =
       DependencyProperty.RegisterAttached("MaxByteLength", typeof(int), typeof(MaxByteAttachedProperty), new PropertyMetadata(OnTextBoxPropertyChanged));

        private static void OnTextBoxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox tb = d as TextBox;
            if (tb == null)
            {
                return;
            }
            tb.TextChanged += Tb_TextChanged;
        }

        private static void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (IsOutMaxByteLength(tb.Text, tb))
            {
                tb.Text = GetPreText(tb);
                tb.Select(tb.Text.Length, 0);
                return;
            }
        }

        public static Encode GetEncodeModel(DependencyObject obj)
        {
            return (Encode)obj.GetValue(EncodeModelProperty);
        }

        public static void SetEncodeModel(DependencyObject obj, Encode value)
        {
            obj.SetValue(EncodeModelProperty, value);
        }

        // Using a DependencyProperty as the backing store for EncodeM. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EncodeModelProperty =
       DependencyProperty.RegisterAttached("EncodeModel", typeof(Encode), typeof(MaxByteAttachedProperty), new PropertyMetadata(Encode.UTF8, OnEncodeModelChanged));
        private static void OnEncodeModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetEM(d, GetEncodeModel(d));
        }

        private static Encoding GetEncodingModel(DependencyObject obj)
        {
            return (Encoding)obj.GetValue(EncodingModelProperty);
        }

        private static void SetEncodingModel(DependencyObject obj, Encoding value)
        {
            obj.SetValue(EncodingModelProperty, value);
        }

        // Using a DependencyProperty as the backing store for EncodingModel. This enables animation, styling, binding, etc...
        private static readonly DependencyProperty EncodingModelProperty =
       DependencyProperty.RegisterAttached("EncodingModel", typeof(Encoding), typeof(MaxByteAttachedProperty), new PropertyMetadata(Encoding.UTF8));

        private static void SetEM(DependencyObject obj, Encode e)
        {
            switch (e)
            {
                case Encode.Default:
                    SetEncodingModel(obj, Encoding.Default);
                    break;
                case Encode.ASCII:
                    SetEncodingModel(obj, Encoding.ASCII);
                    break;
                case Encode.UTF8:
                    SetEncodingModel(obj, Encoding.UTF8);
                    break;
                case Encode.UTF32:
                    SetEncodingModel(obj, Encoding.UTF32);
                    break;
                case Encode.UTF7:
                    SetEncodingModel(obj, Encoding.UTF7);
                    break;
                case Encode.BigEndianUnicode:
                    SetEncodingModel(obj, Encoding.BigEndianUnicode);
                    break;
                case Encode.Unicode:
                    SetEncodingModel(obj, Encoding.Unicode);
                    break;
                default:
                    break;
            }
        }

        private static bool IsOutMaxByteLength(string txt, DependencyObject obj)
        {
            int txtLength = GetEncodingModel(obj).GetBytes(txt).Length;//文本长度
            if (GetMaxByteLength(obj) >= txtLength)
            {
                SetPreText(obj, txt);
                return false;
            }
            return true;
        }
    }
}
