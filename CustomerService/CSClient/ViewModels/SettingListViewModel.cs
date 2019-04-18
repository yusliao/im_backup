using CSClient.Views.Panels;
using IMModels;
using SDKClient.WebAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSClient.ViewModels
{
    /// <summary>
    /// 设置列表VM
    /// </summary>
    public class SettingListViewModel : ListViewModel<SettingViewModel>
    {
        private ObservableCollection<FastReplyTypeModel> _commonTypeItems = new ObservableCollection<FastReplyTypeModel>();
        public ObservableCollection<FastReplyTypeModel> CommonTypeItems
        {
            get { return _commonTypeItems; }
            set { _commonTypeItems = value; this.OnPropertyChanged(); }
        }

        private ObservableCollection<FastReplyTypeModel> _personalTypeItems = new ObservableCollection<FastReplyTypeModel>();
        public ObservableCollection<FastReplyTypeModel> PersonalTypeItems
        {
            get { return _personalTypeItems; }
            set { _personalTypeItems = value; this.OnPropertyChanged(); }
        }

        public SettingListViewModel(IListView view) : base(view)
        {
            //this.LoadDatas();
        }

        public void LoadDatas()
        {
         
            this.Items.Clear();

            if (AppData.MainMV.IsAdmin)
            {
                SettingModel modelCommon = new SettingModel();
                modelCommon.Name = "公共快捷回复设置";
                modelCommon.SettingType = SettingType.FastReply;
                modelCommon.IsChecked = true;
                    
                SettingViewModel vmCommon = new SettingViewModel(modelCommon);
                vmCommon.View = new SetFastReply(new FastReplyViewModel(new FastReplyModel(), true));
                this.Items.Add(vmCommon);
                this.SelectedItem = vmCommon;

                SettingModel modelPersonal = new SettingModel();
                modelPersonal.Name = "个人快捷回复设置";
                modelPersonal.SettingType = SettingType.FastReply;
                SettingViewModel vmPersonal = new SettingViewModel(modelPersonal);
                vmPersonal.View = new SetFastReply(new FastReplyViewModel(new FastReplyModel(), false));
                this.Items.Add(vmPersonal);
            }
            else
            {
                SettingModel modelCommon = new SettingModel();
                modelCommon.Name = "公共快捷回复设置";
                modelCommon.SettingType = SettingType.FastReply;
                SettingViewModel vmCommon = new SettingViewModel(modelCommon);
                vmCommon.View = new SetFastReply(new FastReplyViewModel(new FastReplyModel(), true));

                SettingModel modelPersonal = new SettingModel();
                modelPersonal.Name = "个人快捷回复设置";
                modelPersonal.SettingType = SettingType.FastReply;
                SettingViewModel vmPersonal = new SettingViewModel(modelPersonal);
                vmPersonal.View = new SetFastReply(new FastReplyViewModel(new FastReplyModel(), false));
                this.Items.Add(vmPersonal);

                this.SelectedItem = vmPersonal;
            }                
         
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

        protected override IEnumerable<SettingViewModel> GetSearchResult(string key)
        {
            return null;
        }
    }
}
