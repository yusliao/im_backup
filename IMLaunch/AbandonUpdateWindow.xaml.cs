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
using System.Windows.Shapes;

namespace IMLaunch
{
    /// <summary>
    /// AbandonUpdateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AbandonUpdateWindow : Window
    {
        public event Action OnUpdate;

        public AbandonUpdateWindow()
        {
            InitializeComponent();
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            UpdateViewModel.KillMainProcess();
            Application.Current?.Shutdown(0);
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            UpdateViewModel.KillMainProcess();
            this.OnUpdate?.Invoke();            
        }

        
    }
}
