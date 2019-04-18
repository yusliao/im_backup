using IMModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.ViewModels
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
        /// <summary>
        /// 通用设置
        /// </summary>
        [Description("通用设置")]
        Common,
        /// <summary>
        /// 隐私
        /// </summary>
        [Description("隐私")]
        Privacy,
        /// <summary>
        ///帮助中心 
        /// </summary>
        [Description("帮助中心")]
        Helpcenter,
        /// <summary>
        /// 意见反馈
        /// </summary>
        [Description("意见反馈")]
        Feedback
    }
}
