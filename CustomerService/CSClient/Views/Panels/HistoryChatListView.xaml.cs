using CSClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace CSClient.Views.Panels
{
    /// <summary>
    /// HistoryChatListView.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryChatListView : UserControl, IListView
    {
        public HistoryChatListView()
        {
            UItaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            InitializeComponent();
            this.DataContext = this.ViewModel = new HistoryChatListViewModel(this);
        }

        public ViewModel ViewModel { get; private set; }

        public TaskScheduler UItaskScheduler { get; private set; }

        public IView GetItemView(ViewModel vm)
        {
            return new ChatHistoryView(vm);
        }

       

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox temptbox = sender as TextBox;               
            TextChange[] change = new TextChange[e.Changes.Count];         
            e.Changes.CopyTo(change, 0);                    
            int offset = change[0].Offset;                   
            if (change[0].AddedLength > 0)                                  
            {
                int num;                                                    
                if (temptbox.Text.IndexOf(' ') != -1 || !int.TryParse(temptbox.Text, out num))
                {                                  
                    temptbox.Text = temptbox.Text.Remove(offset, change[0].AddedLength);
                    temptbox.Select(offset, 0);                        
                }
            }
        }
    }
}
