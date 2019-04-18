using IMClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// SettingListView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingListView : UserControl, IListView
    {
        public SettingListView()
        {
            InitializeComponent();

            SettingListViewModel vm = new SettingListViewModel(this);
            this.DataContext = this.ViewModel = vm;
            this.list.PreviewMouseRightButtonDown += (s, e) => { e.Handled = true; }; 
        }

        public ViewModel ViewModel { get; private set; }

        public IView GetItemView(ViewModel vm)
        {            
            return vm.View;
        }

        private void border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SettingViewModel settingVM = (sender as Border).DataContext as SettingViewModel;
            SettingModel model = settingVM.Model as SettingModel;
            if(model.SettingType==SettingType.Helpcenter)
            {
                //Process proc = new System.Diagnostics.Process();
                //proc.StartInfo.FileName = "http://www.baidu.com";
                //proc.Start();
                try
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = @"https://www.kefangapp.com/help.html";
                    proc.Start();
                }
                catch (Exception ex)
                {

                    MessageBox.ShowDialogBox("请设置默认浏览器", isCancelShow :false);
                }
                
            }
        }
    }
}
