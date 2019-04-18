using IMModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSClient.ViewModels
{
    public class SettingViewModel : ViewModel
    {
        public SettingViewModel(SettingModel model) : base(model)
        {

        }
    }

    public class SettingModel : BaseModel
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; this.OnPropertyChanged(); }
        }

        private SettingType _settingType;
        public SettingType SettingType
        {
            get { return _settingType; }
            set { _settingType = value; this.OnPropertyChanged(); }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; this.OnPropertyChanged(); }
        }
    }

    public enum SettingType
    {
        Common,
        FastReply,
    }
}
