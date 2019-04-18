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
    /*....................../´¯/) 
    ......................,/¯../ 
    ...................../..../ 
    .............../´¯/'...'/´¯¯`·¸ 
    ............/'/.../..../......./¨¯\ 
    ..........('(...´...´.... ¯~/'...') 
    ...........\.................'...../ 
    ............''...\.......... _.·´ 
    ..............\..............( 
    ................\.............\*/
    public class FastReplyViewModel : ViewModel
    {
        #region Commands

        private VMCommand _saveCommand;
        /// <summary>
        /// 保存命令
        /// </summary>
        public VMCommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                    _saveCommand = new VMCommand(Save);
                return _saveCommand;
            }
        }

        private VMCommand _cancelCommand;
        /// <summary>
        /// 取消命令
        /// </summary>
        public VMCommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new VMCommand(Cancel);
                return _cancelCommand;
            }
        }

        #endregion

        public bool IsCommonReply { get; set; }

        private FastReplyTypeModel _selectedType;
        public FastReplyTypeModel SelectedType
        {
            get { return _selectedType; }
            set { _selectedType = value; this.OnPropertyChanged(); }
        }

        private ObservableCollection<FastReplyTypeModel> _typeItems = new ObservableCollection<FastReplyTypeModel>();
        public ObservableCollection<FastReplyTypeModel> TypeItems
        {
            get { return _typeItems; }
            set { _typeItems = value; this.OnPropertyChanged(); }
        }

        public FastReplyViewModel(FastReplyModel model, bool isCommonReply) : base(model)
        {
            this.IsCommonReply = isCommonReply;

            if (this.IsCommonReply)
            {
                if (AppData.MainMV.CommonReply == null || AppData.MainMV.CommonReply.data == null)
                {
                    return;
                }
                LoadData(AppData.MainMV.CommonReply.data);
                AppData.MainMV.SettingListVM.CommonTypeItems = this.TypeItems;
            }
            else
            {
                if (AppData.MainMV.PersonalReply == null || AppData.MainMV.PersonalReply.data == null)
                {
                    return;
                }
                LoadData(AppData.MainMV.PersonalReply.data);
                AppData.MainMV.SettingListVM.PersonalTypeItems = this.TypeItems;
            }
        }

        private void LoadData(List<QuickReplycontent.Data> data)
        {
            foreach (var replyType in data)
            {
                FastReplyTypeModel typeModel = new FastReplyTypeModel()
                {
                    TypeId = replyType.quickReplyCateId,
                    TypeName = replyType.quickReplyCate,
                };

                foreach (var item in replyType.quickReplyContent)
                {
                    FastReplyContentModel contentModel = new FastReplyContentModel()
                    {
                        ContentId = item.contentId,
                        Content = item.content,
                        IsAdd = false,
                    };

                    typeModel.TypeItems.Add(contentModel);
                }

                this.TypeItems.Add(typeModel);
            }
            if (this.TypeItems.Count > 0)
            {
                this.SelectedType = this.TypeItems[0];
            }
        }

        private void Save(object obj)
        {
            if(this.SelectedType == null)
            {
                return;
            }

            foreach (var item in this.SelectedType.TypeItems)
            {
                if (string.IsNullOrEmpty(item.Content))
                {
                    continue;
                }
                int editType = item.IsAdd ? 1 : 2;
                var result = SDKClient.SDKClient.Instance.PostQuickReplyContentedit(editType, item.ContentId, item.Content, this.SelectedType.TypeId);
                if (result.success && item.IsAdd)
                {
                    item.ContentId = result.id;
                    item.IsAdd = false;
                }
            }
        }

        private void Cancel(object obj)
        {

        }
    }

    public class FastReplyModel : BaseModel { }

    public class FastReplyTypeModel : BaseModel
    {
        private int _typeId = -1;
        public int TypeId
        {
            get { return _typeId; }
            set { _typeId = value; this.OnPropertyChanged(); }
        }

        private string _typeName = string.Empty;
        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; this.OnPropertyChanged(); }
        }

        private ObservableCollection<FastReplyContentModel> _typeItems = new ObservableCollection<FastReplyContentModel>();
        public ObservableCollection<FastReplyContentModel> TypeItems
        {
            get { return _typeItems; }
            set { _typeItems = value; this.OnPropertyChanged(); }
        }
    }

    public class FastReplyContentModel : BaseModel
    {
        private int _contentId = -1;
        public int ContentId
        {
            get { return _contentId; }
            set { _contentId = value; this.OnPropertyChanged(); }
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return _content; }
            set { _content = value; this.OnPropertyChanged(); }
        }

        private bool _isAdd = true;
        public bool IsAdd
        {
            get { return _isAdd; }
            set { _isAdd = value; this.OnPropertyChanged(); }
        }

    }
}
