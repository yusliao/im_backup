using IMClient.ViewModels;
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

namespace IMClient.Views.Panels
{
    /// <summary>
    /// CommonSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class CommonSettingView : UserControl, IView
    {
        public CommonSettingView()
        {
            InitializeComponent();

            CommonSettingViewModel vm = new CommonSettingViewModel(new CommonSettingModel());
            this.DataContext = this.ViewModel = vm;
        }

        public ViewModel ViewModel { get; private set; }

    }
}
