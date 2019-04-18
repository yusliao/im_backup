using IMCustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace IMClient.Helper
{
    public class TextBoxHelper
    {
        #region 附加属性 IsClearButton  
        /// <summary>  
        /// 附加属性，是否带清空按钮  
        /// </summary>  
        public static readonly DependencyProperty IsClearButtonProperty =
            DependencyProperty.RegisterAttached("IsClearButton", typeof(bool), typeof(TextBoxHelper), new PropertyMetadata(false, ClearText));


        public static bool GetIsClearButton(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsClearButtonProperty);
        }

        public static void SetIsClearButton(DependencyObject obj, bool value)
        {
            obj.SetValue(IsClearButtonProperty, value);
        }

        #endregion

        #region 回调函数和清空输入框内容的实现  
        /// <summary>  
        /// 回调函数若附加属性IsClearButton值为True则挂载清空TextBox内容的函数  
        /// </summary>  
        /// <param name="d">属性所属依赖对象</param>  
        /// <param name="e">属性改变事件参数</param>  
        private static void ClearText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Button btn = d as Button;
            if (d != null && e.OldValue != e.NewValue)
            {
                btn.Click -= ClearTextClicked;
                if ((bool)e.NewValue)
                {
                    btn.Click += ClearTextClicked;
                }
            }
        }

        /// <summary>  
        /// 清空应用该附加属性的父TextBox内容函数  
        /// </summary>  
        /// <param name="sender">发送对象</param>  
        /// <param name="e">路由事件参数</param>  
        public static void ClearTextClicked(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                var parent = VisualTreeHelper.GetParent(btn);
                while (!(parent is TextBox))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                TextBox txt = parent as TextBox;
                if (txt != null)
                {
                    txt.Clear();
                }
            }
        }

        #endregion
    }

    public class TextBlockHelper
    {
        public static bool HasTextTrimmed(object textBlock)
        {
            if(textBlock is SelectableTextBlock selectable)
            {
                //简单处理，特殊情况下可能会不准
                if (selectable.DesiredSize.Width > selectable.ActualWidth - 10)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(textBlock is SuperSelectableTextBlock superSelectable)
            {
                //简单处理，特殊情况下可能会不准
                if (superSelectable.DesiredSize.Width > superSelectable.ActualWidth - 10)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;

            //下面的处理方法需要考虑有图片的情况，暂时弃用
            //Typeface typeface = new Typeface(
            //    textBlock.FontFamily,
            //    textBlock.FontStyle,
            //    textBlock.FontWeight,
            //    textBlock.FontStretch);
            
            //string text = string.Empty;
            //foreach (var item in textBlock.Content)
            //{
            //    if(item is Run run)
            //    {
            //        text += run.Text;
            //    }
            //    else
            //    {
            //        //还得考虑有图片(表情)的情况
            //    }
            //}

            //FormattedText formattedText = new FormattedText(
            //    text,
            //    System.Threading.Thread.CurrentThread.CurrentCulture,
            //    textBlock.FlowDirection,
            //    typeface,
            //    textBlock.FontSize,
            //    textBlock.Foreground);

            //formattedText.MaxTextWidth = textBlock.ActualWidth;

            //bool isTrimmed = formattedText.Height > textBlock.ActualHeight ||
            //                 formattedText.Width > formattedText.MaxTextWidth;
            //return isTrimmed;
        }
    }

    /// <summary>
    /// 动画类，目前只会这种
    /// </summary>
    public class AnimationHelper
    {
        public static void BeginAnimation(Window win)
        {
   
            DoubleAnimation DAnimation = new DoubleAnimation(0,win.Width,new Duration(TimeSpan.FromSeconds(0.5)));
            DAnimation.From = 0;
            DAnimation.To = win.Width;
            DAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));

            Storyboard.SetTarget(DAnimation, win);
            Storyboard.SetTargetProperty(DAnimation, new PropertyPath(Window.WidthProperty));
            Storyboard story = new Storyboard();
            story.Children.Add(DAnimation);
            story.Begin();

            //DoubleAnimation widthAnimation = new DoubleAnimation(0, win.Width, new Duration(TimeSpan.FromSeconds(0.5)));
            //win.BeginAnimation(Window.WidthProperty, widthAnimation, HandoffBehavior.Compose);

            //DoubleAnimation heightAnimation = new DoubleAnimation(0, win.Height, new Duration(TimeSpan.FromSeconds(0.5)));
            //win.BeginAnimation(Window.HeightProperty, heightAnimation, HandoffBehavior.Compose);
        }
    }


    public class SuperTextBlock:TextBlock
    {
        public SuperTextBlock()
        {
            this.SizeChanged += SuperTextBlock_SizeChanged;

        }

        private void SuperTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SuperTextBlock textBlock = sender as SuperTextBlock;
            Size size = e.NewSize;
            if (size.Height > 60)
            {
                this.SizeChanged -= SuperTextBlock_SizeChanged;
                this.Height = 60;
                this.AppendVisibility = Visibility.Visible;
                this.TextTrimming = TextTrimming.WordEllipsis;
            }      
            else
            {
                this.AppendVisibility = Visibility.Collapsed;
            }    
        }

        public Visibility AppendVisibility
        {
            get { return (Visibility)GetValue(AppendVisibilityProperty); }
            set { SetValue(AppendVisibilityProperty, value); }
        }

        public static readonly DependencyProperty AppendVisibilityProperty =
           DependencyProperty.RegisterAttached("AppendVisibility", typeof(Visibility), typeof(SuperTextBlock), new PropertyMetadata(Visibility.Collapsed));

      
    }

}
