using IMClient.Helper;
using IMClient.Views.Panels;
using IMModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace IMClient.ViewModels
{

    public interface IListViewModel
    {
        bool IsChecked { get; set; }
        //IListView View { get; set; }
        //ViewModel SelectedItem { get; set; } 
        //ObservableCollection<ViewModel> Items { get; set; }
        void Search(string key);

        //Visibility SearchVisibility { get; }
    }


    //public abstract class ViewModelCollection<T> : ObservableCollection<T>, IListViewModel  
    //    where T : ViewModel
    //{
    //    private bool _isChecked;
    //    /// <summary>
    //    /// 是否选中
    //    /// </summary>
    //    public bool IsChecked
    //    {
    //        get { return _isChecked; }
    //        set { _isChecked = value; this.OnPropertyChanged(); }
    //    }

    //    public void Search(string key)
    //    { 
    //        this.SearchResults = string.IsNullOrWhiteSpace(key) ? null : GetSearchResult(key);
    //    }

    //    protected abstract IEnumerable<T> GetSearchResult(string key);


    //    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    //    {
    //        base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    //    }
    //}



    /// <summary>
    /// 基础ListViewModel
    /// </summary>
    public abstract class ListViewModel<T> : ViewModel, IListViewModel where T : ViewModel
    {
        private const int searchCount = 5;
        private string _keyWord;
        public ListViewModel(IListView view) : base(view)
        {
            // App.Current.Dispatcher.Invoke(() => { Items = new ObservableCollection<T>(); });
            _items = new ObservableCollection<T>();
            this.View = view;
        }

        private List<int> TempContacts = new List<int>();
        private List<int> TempGroups = new List<int>();
        private List<int> TempBlacks = new List<int>();
        private bool _isChecked;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// 对应的View
        /// </summary>
        public new IListView View { get; private set; }

        private ObservableCollection<T> _items;
        /// <summary>
        /// 子项列表
        /// </summary>
        public ObservableCollection<T> Items
        {
            get { return _items; }
            set { _items = value; this.OnPropertyChanged(); }
        }

        private T _selectedItem;
        /// <summary>
        /// 当前选项
        /// </summary>
        public virtual T SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                PriorSelectedItem = _selectedItem;
                _selectedItem = value;
                if (_selectedItem != null && _selectedItem.View == null)
                {
                    _selectedItem.View = View.GetItemView(value);

                }
                this.OnPropertyChanged();
            }
        }

        private T _priorSelectedItem;
        //前一个选中的项目
        public T PriorSelectedItem
        {
            get { return _priorSelectedItem; }
            protected set { _priorSelectedItem = value; this.OnPropertyChanged(); }
        }

        private T _selectedSearchItem;
        /// <summary>
        /// 当前选项
        /// </summary>
        public virtual T SelectedSearchItem
        {
            get { return _selectedSearchItem; }
            set
            {
                _selectedSearchItem = value; this.OnPropertyChanged();
                if (_selectedSearchItem != null)
                {
                    AppData.IsGlobalSearch = true;
                    this.SelectedItem = _selectedSearchItem;
                    AppData.MainMV.SearchKey = null;
                    this.IsChecked = true;
                    //AppData.MainMV.ListViewModel = AppData.MainMV.ListViewModel;
                }
            }
        }

        private Visibility _searchVisibility = Visibility.Collapsed;
        /// <summary>
        /// 搜索结果面板是否可见
        /// </summary>
        public Visibility SearchVisibility
        {
            get { return _searchVisibility; }
            private set
            {
                _searchVisibility = value;

                if (value == Visibility.Collapsed)
                {
                    this.SelectedSearchItem = null;
                }
                this.OnPropertyChanged();

            }
        }

        private bool _isNullSearchResult;
        public bool IsNullSearchResult
        {
            get { return _isNullSearchResult; }
            set
            {
                _isNullSearchResult = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isShowContactLine;
        public bool IsShowContactLine
        {
            get { return _isShowContactLine; }
            set
            {
                _isShowContactLine = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isShowGroupLine;
        public bool IsShowGroupLine
        {
            get { return _isShowGroupLine; }
            set
            {
                _isShowGroupLine = value;
                this.OnPropertyChanged();
            }
        }

        private int _noViewContactNum;
        /// <summary>
        /// 联系人未显示条数
        /// </summary>
        public int NoViewContactNum
        {
            get { return _noViewContactNum; }
            set
            {
                _noViewContactNum = value;
                this.OnPropertyChanged();
            }
        }

        private int _noViewGroupNum;
        /// <summary>
        /// 群组未显示条数
        /// </summary>
        public int NoViewGroupNum
        {
            get { return _noViewGroupNum; }
            set
            {
                _noViewGroupNum = value;
                this.OnPropertyChanged();
            }
        }

        private int _noViewBlackNum;
        /// <summary>
        /// 黑名单未读条数
        /// </summary>
        public int NoViewBlackNum
        {
            get { return _noViewBlackNum; }
            set
            {
                _noViewBlackNum = value;
                this.OnPropertyChanged();
            }
        }
        private string _searchKeyWord;
        public string SearchKeyWord
        {
            get { return _searchKeyWord; }
            set
            {
                _searchKeyWord = value;
                this.OnPropertyChanged();
            }
        }

        #region 搜索联系人
        private bool _isSearchContactVisibility;
        public bool IsSearchContactVisibility
        {
            get { return _isSearchContactVisibility; }
            set
            {
                _isSearchContactVisibility = value;
                this.OnPropertyChanged();
            }
        }
        private List<ChatViewModel> _searchContactResults;
        public List<ChatViewModel> SearchContactResults
        {
            get { return _searchContactResults; }
            private set
            {
                _searchContactResults = value;
                this.OnPropertyChanged();

                //if (_searchContactResults == null && _searchContactResults.Count() > 0)
                //{
                //    this.IsSearchContactVisibility = true;
                //}
                //else
                //{
                //    this.IsSearchContactVisibility = false;
                //}
            }
        }
        private bool _isShowMoreContactSearch;
        public bool IsShowMoreContactSearch
        {
            get { return _isShowMoreContactSearch; }
            set
            {
                _isShowMoreContactSearch = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isContactExpander;
        public bool IsContactExpander
        {
            get { return _isContactExpander; }
            set
            {
                _isContactExpander = value;
                this.OnPropertyChanged();
            }
        }
        private bool _isMoreSearch;
        public bool IsMoreSearch
        {
            get { return _isMoreSearch; }
            set
            {
                _isMoreSearch = value;
                this.OnPropertyChanged("IsMoreSearch");
            }
        }
        private string _searchContent;
        public string SerarchContent
        {
            get { return _searchContent; }
            set
            {
                this._searchContent = value;
                this.OnPropertyChanged("SerarchContent");
            }
        }

        #endregion

        #region 群搜索
        private bool _isSearchGroupVisibility;
        public bool IsSearchGroupVisibility
        {
            get { return _isSearchGroupVisibility; }
            set
            {
                _isSearchGroupVisibility = value;
                this.OnPropertyChanged();
            }
        }
        private List<ChatViewModel> _searchGroupResults;
        public List<ChatViewModel> SearchGroupResults
        {
            get { return _searchGroupResults; }
            private set
            {
                _searchGroupResults = value;
                this.OnPropertyChanged();

                //if (_searchGroupResults == null && _searchGroupResults.Count() > 0)
                //{
                //    this.IsSearchContactVisibility = true;
                //}
                //else
                //{
                //    this.IsSearchContactVisibility = false;
                //}
            }
        }
        private bool _isShowMoreGroupSearch;
        public bool IsShowMoreGroupSearch
        {
            get { return _isShowMoreGroupSearch; }
            set
            {
                _isShowMoreGroupSearch = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isGroupExpander = true;
        public bool IsGroupExpander
        {
            get { return _isGroupExpander; }
            set
            {
                _isGroupExpander = value;
                this.OnPropertyChanged();
            }
        }
        #endregion
        #region 黑名单搜索

        private bool _isSearchBlackVisibility;
        public bool IsSearchBlackVisibility
        {
            get { return _isSearchBlackVisibility; }
            set
            {
                _isSearchBlackVisibility = value;
                this.OnPropertyChanged();
            }
        }
        private List<ChatViewModel> _searchBlackResults;
        public List<ChatViewModel> SearchBlackResults
        {
            get { return _searchBlackResults; }
            private set
            {
                _searchBlackResults = value;
                this.OnPropertyChanged();

                //if (_searchBlackResults == null && _searchBlackResults.Count() > 0)
                //{
                //    this.IsSearchContactVisibility = true;
                //}
                //else
                //{
                //    this.IsSearchContactVisibility = false;
                //}
            }
        }
        private bool _isShowMoreBlackSearch;
        public bool IsShowMoreBlackSearch
        {
            get { return _isShowMoreBlackSearch; }
            set
            {
                _isShowMoreBlackSearch = value;
                this.OnPropertyChanged();
            }
        }

        private bool _isBlackExpander;
        public bool IsBlackExpander
        {
            get { return _isBlackExpander; }
            set
            {
                _isBlackExpander = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        public VMCommand MoreSearchContactCommand
        {
            get
            {
                return new VMCommand(MoreSearchContact);
            }
        }

        public VMCommand MoreSearchGroupCommand
        {
            get
            {
                return new VMCommand(MoreSearchGroup);
            }
        }

        public VMCommand MoreSearchBlackCommand
        {
            get
            {
                return new VMCommand(MoreSearchBlack);
            }
        }
        /// <summary>
        /// 从服务中查找联系人
        /// </summary>
        public VMCommand SearchServerUserCommand
        {
            get { return new VMCommand(SearchServerUser); }
        }
        private IEnumerable<T> _searchResults;
        /// <summary>
        /// 搜索结果
        /// </summary>
        public IEnumerable<T> SearchResults
        {
            get { return _searchResults; }
            private set
            {
                _searchResults = value;
                this.OnPropertyChanged();

                if (_searchResults == null)
                {
                    this.SearchVisibility = Visibility.Collapsed;
                }
                else
                {
                    this.SearchVisibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 从服务中查找联系人
        /// </summary>
        /// <param name="obj"></param>
        public void SearchServerUser(object obj)
        {
            var _loadHisTask = Task.Run(() =>
            {
                var result = SDKClient.SDKClient.Instance.SearchQuery(_keyWord, isPhoneNo ? "2" : "3").searchResult;
                if (result != null && result.Count > 0)
                {
                    var tempUuser = result[0];
                    UserModel user = AppData.Current.GetUserModel(tempUuser.Id);
                    user.Area = string.Format("{0} {1}", tempUuser.Province, tempUuser.City);
                    user.Name = tempUuser.UserName;

                    user.Sex = tempUuser.Sex.ToString();
                    user.PhoneNumber = tempUuser.MobilePhone;

                    var isHeadImgUpdate = true;
                    if (File.Exists(user.HeadImg))
                    {
                        var info = new FileInfo(user.HeadImg);
                        if (info.Name == tempUuser.Photo)
                        {
                            isHeadImgUpdate = false;
                        }
                    }
                    if (isHeadImgUpdate)
                        user.HeadImg = Helper.ImageHelper.GetFriendFace(tempUuser.Photo, (s) => user.HeadImg = s);
                    user.HeadImgMD5 = tempUuser.Photo;
                    AppData.MainMV.ShowUserBusinessCard(user, true, isPhoneNo ? ApplyFriendSource.PhoneNumSearch : ApplyFriendSource.KeFangNum);
                }
                else
                {
                    AppData.MainMV.TipMessage = "此用户不存在！";
                }
            });
        }
        /// <summary>
        /// 查看更多搜索到的联系人
        /// </summary>
        /// <param name="obj"></param>
        public void MoreSearchContact(object obj)
        {

            if (!string.IsNullOrEmpty(_keyWord))
            {
                TempContacts.Clear();
                if (IsContactExpander)
                {
                    IsContactExpander = false;
                }
                else
                {
                    IsContactExpander = true;
                }
                this.SearchContactResults = GetSearchContactResult(_keyWord, !IsContactExpander).ToList();
                GetChatItemResult(_keyWord, !IsContactExpander);
                ResetGroup();
                IsShowMoreContactSearch = true;
            }
        }
        /// <summary>
        /// 查看更多搜索到的群组
        /// </summary>
        /// <param name="obj"></param>
        public void MoreSearchGroup(object obj)
        {
            if (!string.IsNullOrEmpty(_keyWord))
            {
                TempGroups.Clear();
                if (IsGroupExpander)
                {
                    IsGroupExpander = false;
                }
                else
                {
                    IsGroupExpander = true;
                }

                this.SearchGroupResults = GetSearchGroupResult(_keyWord, !IsGroupExpander).ToList();
                GetChatItemResult(_keyWord, !IsGroupExpander);
                ResetGroup();
                IsShowMoreGroupSearch = true;
            }
        }
        /// <summary>
        /// 查看更多搜索到的黑名单成员
        /// </summary>
        /// <param name="obj"></param>
        public void MoreSearchBlack(object obj)
        {
            if (!string.IsNullOrEmpty(_keyWord))
            {
                TempBlacks.Clear();
                if (IsBlackExpander)
                {
                    IsBlackExpander = false;
                }
                else
                {
                    IsBlackExpander = true;
                }
                this.SearchBlackResults = GetSearchBlackResult(_keyWord, !IsBlackExpander).ToList();
                GetChatItemResult(_keyWord, !IsBlackExpander);
                ResetGroup();
                IsShowMoreBlackSearch = true;
            }
        }
        bool isPhoneNo = false;
        public void Search(string key)
        {
            _keyWord = key;

            TempContacts.Clear();
            TempBlacks.Clear();
            TempGroups.Clear();
            IsMoreSearch = false;
            this.IsNullSearchResult = false;
            //this.SearchResults = string.IsNullOrWhiteSpace(key) ? null : GetSearchResult(key);
            //AppData.MainMV.IsSearchVisibility = false;
            this.SearchContactResults = string.IsNullOrWhiteSpace(key) ? null : GetSearchContactResult(key).ToList();

            this.SearchGroupResults = string.IsNullOrWhiteSpace(key) ? null : GetSearchGroupResult(key).ToList();
            this.SearchBlackResults = string.IsNullOrWhiteSpace(key) ? null : GetSearchBlackResult(key).ToList();

            if (!string.IsNullOrEmpty(key))
                GetChatItemResult(key);
            this.IsShowContactLine = false;
            this.IsShowGroupLine = false;
            this.IsSearchContactVisibility = false;
            this.IsSearchGroupVisibility = false;
            this.IsSearchBlackVisibility = false;
            if (SearchContactResults == null && SearchGroupResults == null && SearchBlackResults == null)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    AppData.MainMV.IsSearchVisibility = true;
                    this.IsNullSearchResult = true;
                }
                return;
            }
            if (SearchContactResults.Count() == 0 && SearchGroupResults.Count() == 0 && SearchBlackResults.Count() == 0)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    AppData.MainMV.IsSearchVisibility = true;
                    this.IsNullSearchResult = true;
                }
                //return;
            }
            if ((this.SearchContactResults == null || this.SearchContactResults.Count == 0) && (this.SearchBlackResults == null || this.SearchBlackResults.Count == 0))
            {
                if (IsPhoneNo(key) && key.Length == 11)
                {

                    IsMoreSearch = true;
                    this.IsNullSearchResult = false;
                    SerarchContent = "查找\"" + key + "\"手机号";
                    SearchKeyWord = key;
                    isPhoneNo = true;
                }
                else if (IsKeFangID(key))
                {
                    IsMoreSearch = true;
                    this.IsNullSearchResult = false;
                    SerarchContent = "查找\"" + key + "\"可访号";
                    SearchKeyWord = key;
                    isPhoneNo = false;
                }
            }
            if (this.IsNullSearchResult)
                return;
            ResetGroup();
        }
        /// <summary>
        /// 设置组
        /// </summary>
        private void ResetGroup()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (this.SearchContactResults != null && this.SearchContactResults.Count > 0)
                {
                    this.SearchVisibility = Visibility.Visible;
                    ICollectionView cvContact = CollectionViewSource.GetDefaultView(this.SearchContactResults);
                    if (cvContact != null)
                    {
                        cvContact.GroupDescriptions.Clear();
                        cvContact.GroupDescriptions.Add(new PropertyGroupDescription("GroupTypeName"));
                    }
                    if (!this.IsSearchContactVisibility)
                        this.IsSearchContactVisibility = true;
                }
                if (this.SearchGroupResults != null && this.SearchGroupResults.Count > 0)
                {
                    this.SearchVisibility = Visibility.Visible;
                    ICollectionView cvGroup = CollectionViewSource.GetDefaultView(this.SearchGroupResults);
                    if (cvGroup != null)
                    {
                        cvGroup.GroupDescriptions.Clear();
                        cvGroup.GroupDescriptions.Add(new PropertyGroupDescription("GroupTypeName"));
                    }
                    if (!this.IsSearchGroupVisibility)
                        this.IsSearchGroupVisibility = true;
                    IsShowContactLine = true;
                }

                if (SearchBlackResults != null && SearchBlackResults.Count > 0)
                {
                    this.SearchVisibility = Visibility.Visible;
                    ICollectionView cvBlack = CollectionViewSource.GetDefaultView(this.SearchBlackResults);
                    if (cvBlack == null)
                    {
                        return;
                    }
                    cvBlack.GroupDescriptions.Clear();
                    cvBlack.GroupDescriptions.Add(new PropertyGroupDescription("GroupTypeName"));
                    if (this.SearchContactResults != null && this.SearchContactResults.Count() > 0 && !IsShowContactLine)
                        IsShowContactLine = true;
                    if (!IsSearchBlackVisibility)
                        IsSearchBlackVisibility = true;
                    IsShowGroupLine = true;
                }

            });
        }

        protected abstract IEnumerable<T> GetSearchResult(string key);
        /// <summary>
        /// 搜索联系人
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<ChatViewModel> GetSearchContactResult(string key, bool isSeeMore = false)
        {
            NoViewContactNum = 0;
            IsShowMoreContactSearch = false;
            var tempSearchChatModelLst = new List<GlobalSearchChatModel>();
            var tempContactItems = AppData.MainMV.FriendListVM.Items.ToList();
            var tempAttentionItems = AppData.MainMV.AttentionListVM.Items.ToList();
            #region 好友列表
            var tempSearchContactItems = tempContactItems.Where(m =>
            {
                var userModel = m.Model as UserModel;

                if (userModel != null && (!string.IsNullOrEmpty(userModel.DisplayName) && userModel.DisplayName.Contains(key) || (!string.IsNullOrEmpty(userModel.Name) && userModel.Name.Contains(key)))
                && (userModel.LinkType == 0 || userModel.LinkType == 2))
                    return true;
                return false;
            }).ToList();
            foreach (var item in tempSearchContactItems)
            {
                if (item.ID < 0) continue;
                var tempItem = tempSearchChatModelLst.FirstOrDefault(m => m.Model.ID == item.ID);
                if (tempItem != null)
                    continue;
                var id = TempContacts.FirstOrDefault(m => m == item.ID);
                if (id > 0)
                    continue;
                ChatModel chatModel = new ChatModel(item.Model as UserModel);
                var searchChatModel = new GlobalSearchChatModel(chatModel);
                searchChatModel.GroupType = SearchGroupType.Contact;
                searchChatModel.GroupTypeName = "联系人";
                searchChatModel.KeyWord = key;
                if (item.Model is UserModel model && !string.IsNullOrEmpty(model.NickName))
                    searchChatModel.SearchExContent = !string.IsNullOrEmpty(model.Name) && model.Name.Contains(key) ? "昵称:" + model.Name : string.Empty;
                searchChatModel.IsFileAssistant = item.IsFileAssistant;
                TempContacts.Add(item.ID);
                if (tempSearchChatModelLst.Count >= searchCount && !isSeeMore)
                {
                    IsShowMoreContactSearch = true;
                    IsContactExpander = true;
                    NoViewContactNum++;
                    continue;
                }
                tempSearchChatModelLst.Add(searchChatModel);
            }
            //if (key.Length == 11 && IsPhoneNo(key))
            //{
            var tempSearchContactPhoneItems = tempContactItems.Where(m =>
            {
                var userModel = m.Model as UserModel;
                if (userModel != null && (!string.IsNullOrEmpty(userModel.PhoneNumber) && userModel.PhoneNumber == key
                || (!string.IsNullOrEmpty(userModel.KfNum) && userModel.KfNum == key))
                && (userModel.LinkType == 0 || userModel.LinkType == 2))
                    return true;
                return false;
            }).ToList();
            if (tempSearchContactPhoneItems.Count > 0)
            {
                foreach (var item in tempSearchContactPhoneItems)
                {
                    var userModel = item.Model as UserModel;
                    var tempItem = tempSearchChatModelLst.FirstOrDefault(m => m.Model.ID == item.ID);
                    if (tempItem != null)
                    {
                        if (userModel.PhoneNumber == key)
                        {
                            tempItem.SearchExContent = "手机号:" + userModel.PhoneNumber;
                        }
                        else if (userModel.KfNum == key)
                        {
                            tempItem.SearchExContent = "可访号:" + userModel.KfNum;
                        }
                        //tempItem.IsShowPhoneNum = true;

                        continue;
                    }
                    var id = TempContacts.FirstOrDefault(m => m == item.ID);
                    if (id > 0)
                        continue;
                    ChatModel chatModel = new ChatModel(item.Model as UserModel);
                    var searchChatModel = new GlobalSearchChatModel(chatModel);
                    searchChatModel.GroupType = SearchGroupType.Contact;
                    searchChatModel.GroupTypeName = "联系人";
                    searchChatModel.KeyWord = key;
                    searchChatModel.IsFileAssistant = item.IsFileAssistant;
                    if (userModel.PhoneNumber == key)
                    {
                        searchChatModel.SearchExContent = "手机号:" + userModel.PhoneNumber;
                    }
                    else if (userModel.KfNum == key)
                    {
                        searchChatModel.SearchExContent = "可访号:" + userModel.KfNum;
                    }
                    //searchChatModel.IsShowPhoneNum = true;
                    TempContacts.Add(item.ID);
                    if (tempSearchChatModelLst.Count >= searchCount && !isSeeMore)
                    {
                        IsShowMoreContactSearch = true;
                        IsContactExpander = true;
                        NoViewContactNum++;
                        continue;
                    }
                    tempSearchChatModelLst.Add(searchChatModel);
                }
            }
            //}

            #endregion

            #region 关注人列表
            var tempSearchAttentionItems = tempAttentionItems.Where(m =>
            {
                var userModel = m.Model as UserModel;
                if (userModel != null && (!string.IsNullOrEmpty(userModel.DisplayName) && userModel.DisplayName.Contains(key) || (!string.IsNullOrEmpty(userModel.Name) && userModel.Name.Contains(key)))
                && (userModel.LinkType == 0 || userModel.LinkType == 3))
                    return true;
                return false;
            });
            foreach (var item in tempSearchAttentionItems)
            {
                var tempItem = tempSearchChatModelLst.FirstOrDefault(m => m.Model.ID == item.ID);
                if (tempItem != null) continue;
                var id = TempContacts.FirstOrDefault(m => m == item.ID);
                if (id > 0)
                    continue;
                ChatModel chatModel = new ChatModel(item.Model as UserModel);
                var searchChatModel = new GlobalSearchChatModel(chatModel);
                searchChatModel.GroupType = SearchGroupType.Contact;
                searchChatModel.GroupTypeName = "联系人";
                searchChatModel.KeyWord = key;
                if (item.Model is UserModel model && !string.IsNullOrEmpty(model.NickName))
                    searchChatModel.SearchExContent = !string.IsNullOrEmpty(model.Name) && model.Name.Contains(key) ? "昵称:" + model.Name : string.Empty;
                TempContacts.Add(item.ID);
                if (tempSearchChatModelLst.Count >= searchCount && !isSeeMore)
                {
                    IsShowMoreContactSearch = true;
                    IsContactExpander = true;
                    NoViewContactNum++;
                    continue;
                }
                tempSearchChatModelLst.Add(searchChatModel);
            }
            //if (key.Length == 11 && IsPhoneNo(key))
            //{
            //    var tempSearchAttentionPhoneItems = tempAttentionItems.Where(m =>
            //    {
            //        var userModel = m.Model as UserModel;
            //        if (userModel != null && !string.IsNullOrEmpty(userModel.PhoneNumber) && userModel.PhoneNumber == key && (userModel.LinkType == 0 || userModel.LinkType == 3))
            //            return true;
            //        return false;
            //    }).ToList();
            //    if (tempSearchAttentionPhoneItems.Count > 0)
            //    {
            //        foreach (var item in tempSearchAttentionPhoneItems)
            //        {
            //            var tempItem = tempSearchChatModelLst.FirstOrDefault(m => m.Model.ID == item.ID);
            //            if (tempItem != null)
            //            {
            //                tempItem.IsShowPhoneNum = true;
            //                continue;
            //            }
            //            var id = TempContacts.FirstOrDefault(m => m == item.ID);
            //            if (id > 0)
            //                continue;
            //            ChatModel chatModel = new ChatModel(item.Model as UserModel);
            //            var searchChatModel = new GlobalSearchChatModel(chatModel);
            //            searchChatModel.GroupType = SearchGroupType.Contact;
            //            searchChatModel.GroupTypeName = "联系人";
            //            searchChatModel.KeyWord = key;
            //            searchChatModel.IsShowPhoneNum = true;
            //            TempContacts.Add(item.ID);
            //            if (tempSearchChatModelLst.Count >= searchCount && !isSeeMore)
            //            {
            //                IsShowMoreContactSearch = true;
            //                IsContactExpander = true;
            //                NoViewContactNum++;
            //                continue;
            //            }
            //            tempSearchChatModelLst.Add(searchChatModel);
            //        }
            //    }
            //}
            #endregion
            if (tempSearchChatModelLst.Count > 0)
            {
                IsSearchContactVisibility = true;
                //if (tempSearchChatModelLst.Count > 5)
                //{
                //    IsShowMoreContactSearch = true;
                //}
                //else
                //    IsShowMoreContactSearch = false;
            }
            else
            {
                IsSearchContactVisibility = false;
                //IsShowMoreContactSearch = false;
            }
            //if (NoViewContactNum > 0)
            //    NoViewContactNum = NoViewContactNum - 1;
            return tempSearchChatModelLst;
        }
        private SearhResultType searchResult(string keyword)
        {
            //if (IsKeFangID(keyword))
            //    return SearhResultType.KeFangId;
            //if(is)
            return SearhResultType.Other;
        }

        /// <summary>
        /// 搜索群组
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<ChatViewModel> GetSearchGroupResult(string key, bool isSeeMore = false)
        {
            IsShowMoreGroupSearch = false;
            NoViewGroupNum = 0;
            var tempSearchChatModelLst = new List<GlobalSearchChatModel>();
            var tempGroupItems = AppData.MainMV.GroupListVM.Items.ToList();
            #region 群组列表
            var tempSearcGroupItems = tempGroupItems.Where(m =>
            {
                var groupModel = m.Model as GroupModel;
                if (groupModel != null && !string.IsNullOrEmpty(groupModel.DisplayName) && groupModel.DisplayName.Contains(key))
                {
                    return true;
                }
                return false;
            }).ToList();
            foreach (var item in tempSearcGroupItems)
            {
                var tempItem = tempSearchChatModelLst.FirstOrDefault(m => m.Model.ID == item.ID);
                if (tempItem != null) continue;
                var id = TempGroups.FirstOrDefault(m => m == item.ID);
                if (id > 0)
                    continue;
                ChatModel chatModel = new ChatModel(item.Model as GroupModel);
                var searchChatModel = new GlobalSearchChatModel(chatModel);
                searchChatModel.GroupType = SearchGroupType.Group;
                searchChatModel.GroupTypeName = "群组";
                searchChatModel.KeyWord = key;
                TempGroups.Add(item.ID);
                if (tempSearchChatModelLst.Count >= searchCount && !isSeeMore)
                {
                    IsShowMoreGroupSearch = true;
                    IsGroupExpander = true;
                    NoViewGroupNum++;
                    continue;
                }
                tempSearchChatModelLst.Add(searchChatModel);
            }
            #endregion

            if (tempSearchChatModelLst.Count > 0)
            {
                IsSearchGroupVisibility = true;
                //if (tempSearchChatModelLst.Count > 5)
                //    IsShowMoreGroupSearch = true;
                //else
                //    IsShowMoreGroupSearch = false;
            }
            else
            {
                IsSearchGroupVisibility = false;
                //IsShowMoreGroupSearch = false;
            }
            //if (NoViewGroupNum > 0)
            //    NoViewGroupNum = NoViewGroupNum - 1;
            return tempSearchChatModelLst;
        }

        /// <summary>
        /// 搜索黑名单
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<ChatViewModel> GetSearchBlackResult(string key, bool isSeeMore = false)
        {
            IsShowMoreBlackSearch = false;
            NoViewBlackNum = 0;
            var tempSearchChatModelLst = new List<GlobalSearchChatModel>();
            var tempBlackItems = AppData.MainMV.BlacklistVM.Items.ToList();
            #region 黑名单列表
            var tempSearcBlacktems = tempBlackItems.Where(m =>
            {
                var userModel = m.Model as UserModel;
                if (!string.IsNullOrEmpty(userModel.DisplayName) && userModel.DisplayName.Contains(key) || (!string.IsNullOrEmpty(userModel.Name) && userModel.Name.Contains(key)))
                    return true;
                return false;
            }).ToList();
            foreach (var item in tempSearcBlacktems)
            {
                var tempItem = tempSearchChatModelLst.FirstOrDefault(m => m.Model.ID == item.ID);
                if (tempItem != null) continue;
                var id = TempBlacks.FirstOrDefault(m => m == item.ID);
                if (id > 0)
                    continue;
                ChatModel chatModel = new ChatModel(item.Model as UserModel);
                var searchChatModel = new GlobalSearchChatModel(chatModel);
                searchChatModel.GroupType = SearchGroupType.Black;
                searchChatModel.GroupTypeName = "黑名单";
                searchChatModel.KeyWord = key;
                if (item.Model is UserModel model && !string.IsNullOrEmpty(model.NickName))
                    searchChatModel.SearchExContent = !string.IsNullOrEmpty(model.Name) && model.Name.Contains(key) ? "昵称:" + model.Name : string.Empty;
                TempBlacks.Add(item.ID);
                if (tempSearchChatModelLst.Count >= searchCount && !isSeeMore)
                {
                    IsShowMoreBlackSearch = true;
                    IsBlackExpander = true;
                    NoViewBlackNum++;
                    continue;
                }
                tempSearchChatModelLst.Add(searchChatModel);
            }
            if (key.Length == 11 && IsPhoneNo(key))
            {
                var tempSearchBlackPhoneItems = tempBlackItems.Where(m =>
                {
                    var userModel = m.Model as UserModel;
                    if (userModel != null && !string.IsNullOrEmpty(userModel.PhoneNumber) && userModel.PhoneNumber == key && (userModel.LinkDelType == 0 || userModel.LinkDelType == 1) && (userModel.LinkType == 1 || userModel.LinkType == 3))
                        return true;
                    return false;
                }).ToList();
                if (tempSearchBlackPhoneItems.Count > 0)
                {
                    foreach (var item in tempSearchBlackPhoneItems)
                    {
                        var userModel = item.Model as UserModel;
                        var tempItem = tempSearchChatModelLst.FirstOrDefault(m => m.Model.ID == item.ID);
                        if (tempItem != null)
                        {
                            //tempItem.IsShowPhoneNum = true;
                            if (userModel.PhoneNumber == key)
                            {
                                tempItem.SearchExContent = "手机号:" + userModel.PhoneNumber;
                            }
                            else if (userModel.KfNum == key)
                            {
                                tempItem.SearchExContent = "可访号:" + userModel.KfNum;
                            }
                            continue;
                        }
                        var id = TempBlacks.FirstOrDefault(m => m == item.ID);
                        if (id > 0)
                            continue;
                        ChatModel chatModel = new ChatModel(item.Model as UserModel);
                        var searchChatModel = new GlobalSearchChatModel(chatModel);
                        searchChatModel.GroupType = SearchGroupType.Black;
                        searchChatModel.GroupTypeName = "黑名单";
                        searchChatModel.KeyWord = key;
                        if (userModel.PhoneNumber == key)
                        {
                            searchChatModel.SearchExContent = "手机号:" + userModel.PhoneNumber;
                        }
                        else if (userModel.KfNum == key)
                        {
                            searchChatModel.SearchExContent = "可访号:" + userModel.KfNum;
                        }
                        TempBlacks.Add(item.ID);
                        if (tempSearchChatModelLst.Count >= 5 && !isSeeMore)
                        {
                            IsShowMoreBlackSearch = true;
                            IsBlackExpander = true;
                            NoViewBlackNum++;
                            continue;
                        }
                        tempSearchChatModelLst.Add(searchChatModel);
                    }
                }
            }
            #endregion
            if (tempSearchChatModelLst.Count > 0)
            {
                IsSearchBlackVisibility = true;
                //if (tempSearchChatModelLst.Count > 5)
                //    IsShowMoreBlackSearch = true;
                //else
                //    IsShowMoreBlackSearch = false;
            }
            else
            {
                IsSearchBlackVisibility = false;
                //IsShowMoreBlackSearch = false;
            }
            //if (NoViewBlackNum > 0)
            //    NoViewBlackNum = NoViewBlackNum - 1;
            return tempSearchChatModelLst;
        }

        public void GetChatItemResult(string key, bool isSeeMore = false)
        {

            var tempItems = AppData.MainMV.ChatListVM.Items.ToList();
            var tempSearchItems = tempItems.Where(info => (!string.IsNullOrEmpty((info.Model as ChatModel).Chat.DisplayName) && (info.Model as ChatModel).Chat.DisplayName.Contains(key)));
            foreach (var item in tempSearchItems)
            {
                if (item.ID < 0) continue;
                if (item.IsGroup)
                {
                    var grouItem = this.SearchGroupResults.FirstOrDefault(m => m.ID == item.ID);
                    if (grouItem != null) continue;
                    var id = TempGroups.FirstOrDefault(m => m == item.ID);
                    if (id > 0)
                        continue;
                    var searchChatModel = new GlobalSearchChatModel(item.Chat);
                    searchChatModel.GroupType = SearchGroupType.Group;
                    searchChatModel.GroupTypeName = "群组";
                    searchChatModel.KeyWord = key;
                    TempGroups.Add(item.ID);
                    if (this.SearchGroupResults.Count >= searchCount && !isSeeMore)
                    {
                        IsShowMoreGroupSearch = true;
                        IsGroupExpander = true;
                        NoViewGroupNum++;
                        continue;
                    }
                    this.SearchGroupResults.Add(searchChatModel);
                }
                else
                {
                    if (item.Chat != null)
                    {
                        var userModel = item.Chat.Chat as UserModel;
                        if (userModel == null) continue;
                        if (userModel.LinkType == 1 || userModel.LinkType == 3)
                        {
                            var blackItem = this.SearchBlackResults.FirstOrDefault(m => m.ID == item.ID);
                            if (blackItem != null) continue;
                            var id = TempBlacks.FirstOrDefault(m => m == item.ID);
                            if (id > 0)
                                continue;
                            var searchChatModel = new GlobalSearchChatModel(item.Chat);
                            searchChatModel.GroupType = SearchGroupType.Black;
                            searchChatModel.GroupTypeName = "黑名单";
                            searchChatModel.KeyWord = key;
                            TempBlacks.Add(item.ID);
                            if (this.SearchBlackResults.Count >= searchCount && !isSeeMore)
                            {
                                IsShowMoreBlackSearch = true;
                                IsContactExpander = true;
                                NoViewBlackNum++;
                                continue;

                            }
                            this.SearchBlackResults.Add(searchChatModel);
                        }
                        else
                        {
                            var contactItem = this.SearchContactResults.FirstOrDefault(m => m.ID == item.ID);
                            if (contactItem != null) continue;
                            var id = TempContacts.FirstOrDefault(m => m == item.ID);
                            if (id > 0)
                                continue;
                            var searchChatModel = new GlobalSearchChatModel(item.Chat);
                            searchChatModel.GroupType = SearchGroupType.Contact;
                            searchChatModel.GroupTypeName = "联系人";
                            searchChatModel.KeyWord = key;
                            searchChatModel.IsFileAssistant = item.IsFileAssistant;
                            TempContacts.Add(item.ID);
                            if (this.SearchContactResults.Count >= searchCount && !isSeeMore)
                            {
                                IsShowMoreContactSearch = true;
                                IsBlackExpander = true;
                                NoViewContactNum++;
                                continue;
                            }
                            this.SearchContactResults.Add(searchChatModel);
                        }
                    }
                }
            }
            //匹配手机号码
            //if (key.Length == 11 && IsPhoneNo(key))
            //{
            //    var tempPhoneItems = tempItems.Where(info =>
            //    {
            //        if (!info.IsGroup && info.Chat != null)
            //        {
            //            var userModel = info.Chat.Chat as UserModel;
            //            if (userModel != null && !string.IsNullOrEmpty(userModel.PhoneNumber) && userModel.PhoneNumber == key)
            //                return true;
            //        }
            //        return false;
            //    });
            //    foreach (var item in tempPhoneItems)
            //    {
            //        var userModel = item.Chat.Chat as UserModel;
            //        if (userModel == null) continue;
            //        if (userModel.LinkType == 1 || userModel.LinkType == 3)
            //        {
            //            var blackItem = this.SearchBlackResults.FirstOrDefault(m => m.ID == item.ID);
            //            if (blackItem != null) continue;
            //            var id = TempBlacks.FirstOrDefault(m => m == item.ID);
            //            if (id > 0)
            //                continue;
            //            var searchChatModel = new GlobalSearchChatModel(item.Chat);
            //            searchChatModel.GroupType = SearchGroupType.Black;
            //            searchChatModel.GroupTypeName = "黑名单";
            //            searchChatModel.KeyWord = key;
            //            searchChatModel.IsShowPhoneNum = true;
            //            TempBlacks.Add(item.ID);
            //            if (this.SearchBlackResults.Count >= searchCount && !isSeeMore)
            //            {
            //                IsShowMoreBlackSearch = true;
            //                IsBlackExpander = true;
            //                NoViewBlackNum++;
            //                continue;

            //            }
            //            this.SearchBlackResults.Add(searchChatModel);
            //        }
            //        else
            //        {
            //            var contactItem = this.SearchContactResults.FirstOrDefault(m => m.ID == item.ID);
            //            if (contactItem != null) continue;
            //            var id = TempContacts.FirstOrDefault(m => m == item.ID);
            //            if (id > 0)
            //                continue;
            //            var searchChatModel = new GlobalSearchChatModel(item.Chat);
            //            searchChatModel.GroupType = SearchGroupType.Contact;
            //            searchChatModel.GroupTypeName = "联系人";
            //            searchChatModel.KeyWord = key;
            //            searchChatModel.IsShowPhoneNum = true;
            //            TempContacts.Add(item.ID);
            //            if (this.SearchContactResults.Count >= searchCount && !isSeeMore)
            //            {
            //                IsShowMoreContactSearch = true;
            //                IsBlackExpander = true;
            //                NoViewContactNum++;
            //                continue;
            //            }
            //            this.SearchContactResults.Add(searchChatModel);
            //        }
            //    }
            //}
            //NoViewBlackNum;
            //NoViewContactNum;
            //NoViewGroupNum;
        }
        /// <summary>
        /// 判断输入的字符串是否是一个合法的手机号
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsPhoneNo(string str_handset)
        {
            return Regex.IsMatch(str_handset, "^(0\\d{2,3}-?\\d{7,8}(-\\d{3,5}){0,1})|(((13[0-9])|(15([0-3]|[5-9]))|(18[0-9])|(17[0-9])|(14[0-9]))\\d{8})$");
        }

        /// <summary>
        /// 可访号（6-16位英文、数字、下划线且首位为字母）
        /// </summary>
        /// <param name="keyWord">搜索关键字</param>
        /// <returns></returns>
        public static bool IsKeFangID(string keyWord)
        {
            if (keyWord.Length < 6)
                return false;
            else if (keyWord.Length > 16)
            {
                return false;
            }
            else
            {
                string pattern = "^[a-zA-Z][a-zA-Z0-9_]*$";
                var isChinese = Regex.IsMatch(keyWord, pattern);
                return isChinese;
            }
        }
        //public IEnumerable<T> GetSearchContactResult(string key)
        //{

        //}

        //public IEnumerable<T> GetSearchGroupResult(string key)
        //{

        //}
        //public IEnumerable<T> GetSearchBlackResult(string key)
        //{

        //}

    }
    public enum SearhResultType
    {
        PhoneNum,
        KeFangId,
        Nickname,
        Other
    }
}
