using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IMClient.Helper;
using IMModels;
using SDKClient.Model;
using SDKClient.WebAPI;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 用户查找列表View
    /// </summary>
    public class UserSearchListViewModel : ListViewModel<UserViewModel>
    {
        public event Action OnResetScrollViewerVerticalOffset;

        /// <summary>
        /// 查找用户列表View
        /// </summary>
        /// <param name="view"></param>
        public UserSearchListViewModel(IListView view) : base(view)
        {

        }

        #region 搜索
        protected override IEnumerable<UserViewModel> GetSearchResult(string key)
        {
            return null;
        }
        //protected override IEnumerable<UserViewModel> GetSearchContactResult(string key)
        //{
        //    return null;
        //}

        //protected override IEnumerable<UserViewModel> GetSearchBlackResult(string key)
        //{
        //    return null;
        //}
        //protected override IEnumerable<UserViewModel> GetSearchGroupResult(string key)
        //{
        //    return null;
        //}
        #endregion
        private VMCommand _searchCommand;
        /// <summary>
        /// 跳转命令
        /// </summary> 
        public VMCommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                    _searchCommand = new VMCommand(Search, new Func<object, bool>(o => o != null));
                return _searchCommand;
            }
        }
        private bool _isSearchUserData = false;
        /// <summary>
        /// 是否搜索到相关数据
        /// </summary>
        public bool IsSearchUserData
        {
            get
            {
                return _isSearchUserData;
            }
            set
            {
                _isSearchUserData = value;
                this.OnPropertyChanged("IsSearchUserData");

            }
        }

        private void Search(object para)
        {
            this.Items.Clear();
            _isLoadData = false;
            this._searchKey = string.Format("{0}", para).Trim().Replace(" ", "");
            IsSearchUserData = false;
            if (this._searchKey.Length > 0)
            {
                IsDataLoading = true;
                this.OnResetScrollViewerVerticalOffset?.Invoke();
                this._currentPageIndex = 0;
                this.GetData();

                //var result = SDKClient.SDKClient.Instance.SearchQuery(key, "1,2", 1, PageSize).searchResult;
                //_currentPageIndex = 1;
                //LoadData(result);
                //SDKClient.SDKClient.Instance.SearchNewFriend(key);
            }
        }

        private const int PageSize = 20;
        private int _currentPageIndex = 0;
        private string _searchKey;
        private bool _isLoadData;
        private bool _isDataLoading;
        public bool IsDataLoading
        {
            get { return _isDataLoading; }
            set
            {
                _isDataLoading = value;
                OnPropertyChanged("IsDataLoading");
            }
        }

        public void GetData()
        {
            if (string.IsNullOrEmpty(this._searchKey))
            {
                return;
            }
            if (_isLoadData) return;
            this._currentPageIndex++;
            //AsyncHandler.CallFuncWithUI(System.Windows.Application.Current.Dispatcher, () =>
            //{
            //    _isLoadData = true;
            //    var result = SDKClient.SDKClient.Instance.SearchQuery(this._searchKey, "1,2", this._currentPageIndex, PageSize).searchResult;
            //    return result;
            //}, (ex, datas) =>
            //{
            //    IsDataLoading = false;
            //    if (datas != null)
            //    {
            //        this.LoadData(datas);
            //        _isLoadData = false;
            //    }
            //    else if (_currentPageIndex == 1)
            //    {
            //        IsSearchUserData = true;
            //    }
            //});
            var result = new List<SearchInfo>();
            var loadTask = Task.Run(() =>
            {
                _isLoadData = true;
                result = SDKClient.SDKClient.Instance.SearchQuery(this._searchKey, "1,2,3", this._currentPageIndex, PageSize).searchResult.ToList();
            }).ContinueWith(t =>
            {
                if (result != null && result.Count > 0)
                    this.LoadData(result);
                else if (_currentPageIndex == 1)
                    IsSearchUserData = true;
                IsDataLoading = false;
                _isLoadData = false;
            });
        }

        public void LoadData(IList<SearchInfo> datas)
        {
            var isDisplayAddFriend = IsPhoneNo(_searchKey) || IsKeFangID(_searchKey) ? true : false;
            if (datas != null && datas.Count > 0)
            {
                foreach (var d in datas)
                {
                    if (d.Id == AppData.Current.LoginUser.User.ID)
                    {
                        continue;
                    }
                    UserModel user = AppData.Current.GetUserModel(d.Id);
                    user.Area = string.Format("{0} {1}", d.Province, d.City);
                    user.Name = d.UserName;
                    //user.NickName = d.NickName;
                    //user.DisplayName = string.IsNullOrEmpty(d.NickName) ? d.UserName : d.NickName;
                    user.Sex = d.Sex.ToString();
                    user.PhoneNumber = d.MobilePhone;
                    user.KfNum = d.kfId;
                    //App.Current.Dispatcher.BeginInvoke(new Action(() =>
                    //{
                    var isHeadImgUpdate = true;
                    if (File.Exists(user.HeadImg))
                    {
                        var info = new FileInfo(user.HeadImg);
                        if (info.Name == d.Photo)
                        {
                            isHeadImgUpdate = false;
                        }
                    }
                    if (isHeadImgUpdate)
                        user.HeadImg = Helper.ImageHelper.GetFriendFace(d.Photo, (s) => user.HeadImg = s);
                    //}));
                    user.HeadImgMD5 = d.Photo;
                    if (AppData.MainMV.AttentionListVM.Items.Count > 0)
                    {
                        var attentionList = AppData.MainMV.AttentionListVM.Items.ToList();
                        var tempAttention = attentionList.FirstOrDefault(m => m.Model.ID == d.Id);
                        if (tempAttention == null && user.IsAttention)
                            user.IsAttention = false;
                    }
                    if (AppData.MainMV.BlacklistVM != null && AppData.MainMV.BlacklistVM.Items.Count > 0)
                    {
                        var blackUser = AppData.MainMV.BlacklistVM.Items.ToList().FirstOrDefault(m => m.Model.ID == user.ID);
                        if (blackUser != null && (blackUser.Model as UserModel).AttentionID != 0)
                        {
                            user.IsAttention = true;
                        }
                    }
                    UserViewModel userVM = new UserViewModel(user);
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (isDisplayAddFriend && (d.MobilePhone == _searchKey || d.kfId == _searchKey))
                        {
                            if (user.PhoneNumber == _searchKey)
                            {
                                userVM.NumPhone = "手机号：" + user.PhoneNumber;
                                userVM.ApplyFriendSourceType = ApplyFriendSource.PhoneNumSearch;
                            }
                            else
                            {
                                userVM.NumPhone = "可访号：" + user.KfNum;
                                userVM.ApplyFriendSourceType = ApplyFriendSource.KeFangNum;
                            }
                            userVM.IsAddFriend = true;

                        }
                        var tempUser = this.Items.ToList().FirstOrDefault(m => m.ID == userVM.ID);
                        if (tempUser == null)
                            this.Items.Add(userVM);
                        else
                        {

                        }
                    }));
                }
            }
        }

        public void LoadData(SearchNewFriendPackage pg)
        {
            if (pg == null || pg.data == null || pg.data.items == null)
            {
                return;
            }
            foreach (var d in pg.data.items)
            {
                UserModel user = AppData.Current.GetUserModel(d.strangerId);
                user.Area = string.Format("{0} {1}", d.province, d.city);
                user.Name = d.strangerName;
                user.Sex = d.sex.ToString();

                user.HeadImgMD5 = d.strangerPhoto;
                UserViewModel userVM = new UserViewModel(user);
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.Items.Add(userVM);
                    user.HeadImg = Helper.ImageHelper.GetFriendFace(d.strangerPhoto, (s) => user.HeadImg = s);
                }));
            }
        }
    }

}
