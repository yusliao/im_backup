using IMModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.ViewModels
{
    public class PrivacyViewModel : ViewModel
    {
        private string _selectedReceiveMode;
        public string SelectedReceiveMode
        {
            get { return _selectedReceiveMode; }
            set { _selectedReceiveMode = value; this.OnPropertyChanged(); }
        }

        private ObservableCollection<string> _receiveStrangerMessageMode;
        public ObservableCollection<string> ReceiveStrangerMessageMode
        {
            get { return _receiveStrangerMessageMode; }
            set { _receiveStrangerMessageMode = value; this.OnPropertyChanged(); }
        }

        public PrivacyViewModel(PrivacyModel model) : base(model)
        {
            this.ReceiveStrangerMessageMode = new ObservableCollection<string>();
            ReceiveStrangerMessageMode.Add("不接收");
            SelectedReceiveMode = "接收";
            AppData.Current.LoginUser.User.IsReceiveStrangerMessage = true;

            AppData.MainMV.OnUpdatePrivacySetting += MainMV_OnUpdatePrivacySetting;
        }

        private void MainMV_OnUpdatePrivacySetting()
        {
            App.Current.Dispatcher.Invoke(() => 
            {
                if (!AppData.Current.LoginUser.User.IsReceiveStrangerMessage)
                {
                    ReceiveStrangerMessageMode.Clear();
                    ReceiveStrangerMessageMode.Add("接收");
                    SelectedReceiveMode = "不接收";
                }
                else
                {
                    ReceiveStrangerMessageMode.Clear();
                    ReceiveStrangerMessageMode.Add("不接收");
                    SelectedReceiveMode = "接收";
                }
            });            
        }
    }

    public class PrivacyModel : BaseModel
    {

    }
}
