using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// 控制Popup的ToggleButton
    /// </summary>
    public class PopupToggleButton : ToggleButton
    {
        static PopupToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupToggleButton), new FrameworkPropertyMetadata(typeof(PopupToggleButton)));
        }

        public static DependencyProperty PopupProperty = DependencyProperty.Register("Popup",
            typeof(Popup), typeof(PopupToggleButton), new PropertyMetadata(OnPopupPropertyChanged));

        public static void OnPopupPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PopupToggleButton ptb = d as PopupToggleButton;
            ptb.SetPopup(e.NewValue as Popup);
        }


        public PopupToggleButton()
        { 
            this.Checked += PopupToggleButton_Checked;
            this.Unchecked += PopupToggleButton_Checked;
        }
         
        private void PopupToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsChecked == true &&this.Popup!=null)
            {
                this.IsHitTestVisible = false;
            }
            else
            {
                this.IsHitTestVisible = true;
            } 
        }

        private void SetPopup(Popup popup)
        {
            try
            {
                if (popup != null)
                {
                    Binding binding = new Binding("IsChecked");
                    binding.Source = this;
                    binding.Mode = BindingMode.OneWay;
                    popup.SetBinding(Popup.IsOpenProperty, binding);
                    popup.Closed += Popup_Closed;
                }
            }
            catch
            {

            } 
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            this.SetValue(ToggleButton.IsCheckedProperty, false);
            //this.IsChecked = false;
        }

        /// <summary>
        /// 目标Popup
        /// </summary>
        public Popup Popup
        {
            get { return (Popup)GetValue(PopupProperty); }
            set { SetValue(PopupProperty, value); }
        }
    }
}
