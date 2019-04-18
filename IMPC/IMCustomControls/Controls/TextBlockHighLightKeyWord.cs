using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace IMCustomControls
{
    public class TextBlockHighLightKeyWord : TextBlock
    {
        public TextBlockHighLightKeyWord()
        {

        }
        public static readonly DependencyProperty SearchWordProperty = DependencyProperty.Register("SearchWord", typeof(string), typeof(TextBlockHighLightKeyWord),
               new PropertyMetadata(OnSearchWordPropertyChanged));
        public string SearchWord
        {
            get { return (string)GetValue(SearchWordProperty); }
            set { SetValue(SearchWordProperty, value); }
        }
        private static void OnSearchWordPropertyChanged(DependencyObject obj,DependencyPropertyChangedEventArgs s)
        {
           var textBlock = obj as TextBlockHighLightKeyWord;
            if (s.NewValue == null) return;
            var keyWord = s.NewValue.ToString();
            if (string.IsNullOrEmpty(keyWord)) return;
            int starIndex = -1;
            //if (!InputIsChinese(keyWord) && !InputIsNum(keyWord))
            //{
            //    if (!string.IsNullOrEmpty(pinYin))
            //        starIndex = pinYin.IndexOf(keyWord);
            //}
            //else
             
                starIndex = textBlock.Text.IndexOf(keyWord);
            if (starIndex != -1)
            {
                TextBlock tb = (TextBlock)obj;
                TextEffect tfe = new TextEffect();
                tfe.Foreground = new SolidColorBrush(Color.FromRgb(31,206,195));
                tfe.PositionStart = starIndex;
                tfe.PositionCount = keyWord.Length;
                tb.TextEffects = new TextEffectCollection();
                tb.TextEffects.Add(tfe);
            }

        }
        /// <summary>
        /// 判断输入的是否是汉字
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static bool InputIsChinese(string strText)
        {
            bool result = false;
            Regex reg = new Regex("^[\u4e00-\u9fa5]$");//验证是否输入汉字
            for (int i = 0; i < strText.Length; i++)
            {
                if (reg.IsMatch(strText.Substring(i, 1)))
                    result = true;
                else
                    result = false;
            }
            return result;
        }
        /// <summary>
        /// 判断是否是数字
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static bool InputIsNum(string strText)
        {
            int num = 0;
            return int.TryParse(strText, out num);
        }
    }
}