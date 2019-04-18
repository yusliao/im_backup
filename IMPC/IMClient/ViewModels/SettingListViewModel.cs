using IMClient.Views.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 设置列表VM
    /// </summary>
    public class SettingListViewModel : ListViewModel<SettingViewModel>
    {
        private static SettingViewModel _commonSetting;
        public static SettingViewModel CommonSetting
        {
            get
            {
                if (_commonSetting == null)
                {
                    SettingModel settingModel = new SettingModel()
                    {
                        Name = "通用设置",
                        SettingType = SettingType.Common,
                    };

                    _commonSetting = new SettingViewModel(settingModel) { };
                }
                return _commonSetting;
            }
        }

        private static SettingViewModel _privacy;
        public static SettingViewModel Privacy
        {
            get
            {
                if (_privacy == null)
                {
                    SettingModel settingModel = new SettingModel()
                    {
                        Name = "隐       私",
                        SettingType = SettingType.Privacy,
                    };

                    _privacy = new SettingViewModel(settingModel) { };
                }
                return _privacy;
            }
        }

        private static SettingViewModel _helpCenter;
        public static SettingViewModel HelpCenter
        {
            get
            {
                if (_helpCenter == null)
                {
                    SettingModel setting = new SettingModel()
                    {
                        Name = "帮助中心",
                        SettingType = SettingType.Helpcenter
                    };
                    _helpCenter = new SettingViewModel(setting) { };
                }
                return _helpCenter;

            }
        }

        private static SettingViewModel _feedBack;
        public static SettingViewModel FeedBack
        {
            get
            {
                if (_feedBack == null)
                {
                    SettingModel setting = new SettingModel()
                    {
                        Name = "意见反馈",
                        SettingType = SettingType.Feedback
                    };
                    _feedBack = new SettingViewModel(setting) { };
                }
                return _feedBack;

            }
        }

        public SettingListViewModel(IListView view) : base(view)
        {

        }

        public void LoadDatas()
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.Items.Clear();
                this.Items.Add(CommonSetting);
                this.Items.Add(Privacy);
                //this.Items.Add(HelpCenter);
                this.Items.Add(FeedBack);

                this.SelectedItem = CommonSetting;
            }));
        }

        private SettingViewModel _selectedItem;
        public override SettingViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                this.PriorSelectedItem = _selectedItem;
                _selectedItem = value;
                if (_selectedItem != null)
                {
                    if (_selectedItem.View == null)
                    {
                        _selectedItem.View = View.GetItemView(value);
                    }
                }

                this.OnPropertyChanged();
            }
        }

        #region 搜索
        protected override IEnumerable<SettingViewModel> GetSearchResult(string key)
        {
            return null;
        }
        //protected override IEnumerable<SettingViewModel> GetSearchContactResult(string key)
        //{
        //    return null;
        //}

        //protected override IEnumerable<SettingViewModel> GetSearchBlackResult(string key)
        //{
        //    return null;
        //}
        //protected override IEnumerable<SettingViewModel> GetSearchGroupResult(string key)
        //{
        //    return null;
        //}
        #endregion
    }
}
