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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CSClient.Views.ChildWindows
{
    /// <summary>
    /// AppendWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AppendWindow : Window
    {
        public static bool AutoClose { get; set; }
        private bool _isClosing = false;

        Storyboard _load, _unload;

        object _content;

        public AppendWindow(Rect rect, bool isInner,object content)
        {
            InitializeComponent();
            AutoClose = true;
            this.Loaded += AppendWindow_Loaded; 
            this.Closed += AppendWindow_Closed;

            App.Current.Deactivated += Hide;
            App.Current.MainWindow.Activated += MainWindow_Activated;
            App.Current.MainWindow.PreviewMouseDown += MainWindow_PreviewMouseDown;
            ViewModels.MainViewModel.CloseAppend += MainViewModel_CloseAppend;

            Owner = App.Current.MainWindow;
            _content = content;
            if (isInner)
            { 
                rect.X += 10;
                this.gridLayout.Margin = new Thickness(0, 0, 0, 0);

                _load = this.Resources["InLoading"] as Storyboard;
                _unload = this.Resources["InUnloading"] as Storyboard;
            }
            else
            {
                rect.X -= 1;
                this.gridLayout.Margin = new Thickness(-10, 0, 0, 0);


                _load = this.Resources["OutLoading"] as Storyboard;
                _unload = this.Resources["OutUnloading"] as Storyboard;
            }
            _isClosing = false;
            this.Left = rect.Left;
            this.Top = rect.Top-10;
            this.Width = rect.Width;
            this.Height = rect.Height+20; 
        }

        private void MainWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        { 
            if (App.Current.MainWindow.IsActive)
            { 
                //DoClose();
            }
        }

        private void AppendWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Activate(); this.ccContent.Content = _content;
            if (_load != null)
            {
                _load.Completed += delegate
                {
                    //this.ccContent.Content = _content;
                };
                _load.Begin();
            }
            else
            {
                //this.ccContent.Content = _content;
            } 
        }
         

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            //this.Hide(sender, e);
        }

        private void MainViewModel_CloseAppend()
        {
            DoClose();
        }

        private void Hide(object sender, EventArgs e)
        {
            DoClose();
        }


        private void DoClose()
        {
            if (AutoClose)
            { 
                App.Current.Deactivated -= Hide;  
                App.Current.MainWindow.Activated -= MainWindow_Activated;
                ViewModels.MainViewModel.CloseAppend -= MainViewModel_CloseAppend;

                App.Current.MainWindow.PreviewMouseDown -= MainWindow_PreviewMouseDown;
                if (!_isClosing)
                {

                    _isClosing = true;
                    if (_unload != null)
                    {
                        _unload.Completed += delegate { this.Close(); };

                        _unload.Begin();
                    }
                    else
                    { 
                        this.Close();
                    } 
                }
            } 
        }

        private void AppendWindow_Closed(object sender, EventArgs e)
        {
            //App.Current.MainWindow.Focus();
        }
  
       
    }
}
