using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IMModels;
using SDKClient.Model;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 查找用户列表View
    /// </summary>
    public class SearchUserListViewModel : ListViewModel<UserViewModel>
    {
        /// <summary>
        /// 查找用户列表View
        /// </summary>
        /// <param name="view"></param>
        public SearchUserListViewModel(IListView view) : base(view)
        {

        }

        protected override IEnumerable<UserViewModel> GetSearchResult(string key)
        {
            return null;
        }



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


        private VMCommand _showBusinessCardCommand;
        /// <summary>
        /// 个人名片命令
        /// </summary> 
        public VMCommand ShowBusinessCardCommand
        {
            get
            {
                if (_showBusinessCardCommand == null)
                    _showBusinessCardCommand = new VMCommand(AppData.MainMV.ShowBusinessCard, new Func<object, bool>(o => o != null));
                return _showBusinessCardCommand;
            }
        }



        private void Search(object para)
        {
            this.Items.Clear();

            string key = string.Format("{0}", para).Trim();
            if (key.Length > 0)
            {
                var result = SDKClient.SDKClient.Instance.SearchQuery(key,"1,2",1,20).searchResult;
                LoadData(result);
                //SDKClient.SDKClient.Instance.SearchNewFriend(key);
            }
        }

        public void LoadData(IList<SearchInfo> datas)
        {
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
                    user.NickName = d.NickName;
                    user.DisplayName = d.NickName == null ? d.UserName : d.NickName;
                    user.Sex = d.Sex.ToString();
                    user.HeadImg = Helper.ImageHelper.GetFriendFace(d.Photo, (s) => user.HeadImg = s); ;
                    user.HeadImgMD5 = d.Photo;

                    UserViewModel userVM = new UserViewModel(user);

                    this.Items.Add(userVM);
                }
            }
        }

        public void LoadData(SearchNewFriendPackage pg)
        {
            if(pg==null || pg.data == null || pg.data.items == null)
            {
                return;
            }
            foreach(var d in pg.data.items)
            {
                UserModel user = AppData.Current.GetUserModel(d.strangerId); 
                user.Area = string.Format("{0} {1}", d.province,d.city);
                user.Name = d.strangerName;
                user.Sex = d.sex.ToString();
                user.HeadImg = Helper.ImageHelper.GetFriendFace(d.strangerPhoto, (s) => user.HeadImg = s); ;
                user.HeadImgMD5 = d.strangerPhoto;
 
                UserViewModel userVM = new UserViewModel(user);

                this.Items.Add(userVM); 
            }
        }
    }
     
}
