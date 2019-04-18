using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IMCustomControls
{ 
    /// <summary>
    /// 图标状态按钮
    /// 支持：默认、鼠标进入、按下、选中、禁用
    /// </summary>
    public class IconStateButton : RadioButton
    {
        static IconStateButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconStateButton), new FrameworkPropertyMetadata(typeof(IconStateButton)));
        }

        #region DependencyProperty.Register


        public static DependencyProperty IconDockProperty = DependencyProperty.Register("IconDock",
            typeof(Dock), typeof(IconStateButton),new PropertyMetadata(Dock.Left));
         
        public static DependencyProperty IconNormalProperty = DependencyProperty.Register("IconNormal",
            typeof(ImageSource), typeof(IconStateButton)); 

        public static DependencyProperty IconMouseOverProperty = DependencyProperty.Register("IconMouseOver",
           typeof(ImageSource), typeof(IconStateButton));  

        public static DependencyProperty IconCheckedProperty = DependencyProperty.Register("IconChecked",
           typeof(ImageSource), typeof(IconStateButton));

        public static DependencyProperty IconDisabledProperty = DependencyProperty.Register("IconDisabled",
          typeof(ImageSource), typeof(IconStateButton));

        #endregion

        #region DependencyProperty.Statement

        /// <summary>
        /// 图标位置
        /// </summary>
        public Dock IconDock
        {
            get { return (Dock)GetValue(IconDockProperty); }
            set { SetValue(IconDockProperty, value); }
        } 


        /// <summary>
        /// 默认图标
        /// </summary>
        public ImageSource IconNormal
        {
            get { return (ImageSource)GetValue(IconNormalProperty); }
            set { SetValue(IconNormalProperty, value); }
        }

      

        /// <summary>
        /// 光标停留图标
        /// </summary>
        public ImageSource IconMouseOver
        {
            get { return (ImageSource)GetValue(IconMouseOverProperty); }
            set { SetValue(IconMouseOverProperty, value); }
        }
         
        /// <summary>
        /// 选中图标
        /// </summary>
        public ImageSource IconChecked
        {
            get { return (ImageSource)GetValue(IconCheckedProperty); }
            set { SetValue(IconCheckedProperty, value); }
        }

        /// <summary>
        /// 禁用图标
        /// </summary>
        public ImageSource IconDisabled
        {
            get { return (ImageSource)GetValue(IconDisabledProperty); }
            set { SetValue(IconDisabledProperty, value); }
        }

        #endregion
    }
}
