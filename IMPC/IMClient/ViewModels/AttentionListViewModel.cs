using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using IMModels;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 关注列表VM
    /// </summary>
    public class AttentionListViewModel : ListViewModel<AttentionVIewModel>
    {
        /// <summary>
        /// 关注列表VM
        /// </summary>
        /// <param name="view"></param>
        public AttentionListViewModel(IListView view) : base(view)
        {
        }
        #region 搜索
        protected override IEnumerable<AttentionVIewModel> GetSearchResult(string key)
        {
            return this.Items.ToList().Where(info => (info.Model as UserModel).DisplayName.Contains(key));
        }
        //protected override IEnumerable<AttentionVIewModel> GetSearchContactResult(string key)
        //{
        //    return null;
        //}

        //protected override IEnumerable<AttentionVIewModel> GetSearchBlackResult(string key)
        //{
        //    return null;
        //}
        //protected override IEnumerable<AttentionVIewModel> GetSearchGroupResult(string key)
        //{
        //    return null;
        //}
        #endregion
        /// <summary>
        /// 加载关注列表
        /// </summary>
        /// <param name="pg"></param>
        public void LoadDatas(SDKClient.Model.GetAttentionListPackage pg)
        {
            if (pg == null || pg.code != 0 || pg.data == null || pg.data.items == null)
            {
                return;
            }
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.Items.Clear();
            }));

            foreach (var item in pg.data.items)
            {
                UserModel user = AppData.Current.GetUserModel(item.strangerId);

                user.DisplayName = user.Name = item.strangerName;
                user.Area = string.Format("{0} {1}", item.province, item.city);
                user.IsAttention = true;
                user.AttentionID = item.strangerLinkId;

                AttentionVIewModel vm = new AttentionVIewModel(user);
                vm.FirstChar = Helper.CommonHelper.GetFirstChar(user.DisplayName);
                vm.GroupByChar = Helper.CommonHelper.GetFirstChar(user.DisplayName, true);

                if (user.LinkType == 1 || user.LinkType == 3)
                {
                    vm.FirstChar = vm.GroupByChar = ' ';
                }

                if (string.IsNullOrEmpty(user.PhoneNumber))
                {
                    //SDKClient.SDKClient.Instance.GetUser(item.strangerId);
                }
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var isHeadImgUpdate = true;
                    if (!user.HeadImg.Equals(IMAssets.ImageDeal.DefaultHeadImage))
                    {
                        if (File.Exists(user.HeadImg))
                        {
                            var info = new FileInfo(user.HeadImg);
                            if (info.Name == item.strangerPhoto)
                            {
                                isHeadImgUpdate = false;
                            }
                        }
                    }

                    if (isHeadImgUpdate)
                        user.HeadImg = IMClient.Helper.ImageHelper.GetFriendFace(item.strangerPhoto, (s) => user.HeadImg = s);
                    var tempAttentionUser = this.Items.ToList().FirstOrDefault(m => m.ID == vm.ID);
                    if (tempAttentionUser == null)
                        this.Items.Add(vm);
                }));
            }

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                List<UserViewModel> tempAttentions = AppData.MainMV.SearchUserListVM.Items.ToList();
                List<AttentionVIewModel> tempAttentionList = this.Items.ToList();
                List<int> attentionList = new List<int>();
                tempAttentionList.ForEach(x =>
                {
                    UserModel user = x.Model as UserModel;
                    attentionList.Add(user.ID);
                });
                if (tempAttentionList.Count > 0 && tempAttentions.Count > 0)
                {
                    foreach (UserViewModel child in tempAttentions)
                    {
                        UserModel user = child.Model as UserModel;
                        if (AppData.MainMV.BlacklistVM != null && AppData.MainMV.BlacklistVM.Items.Count > 0)
                        {
                            var blackUser = AppData.MainMV.BlacklistVM.Items.ToList().FirstOrDefault(m => m.Model.ID == user.ID);
                            if (blackUser != null && (blackUser.Model as UserModel).AttentionID != 0)
                            {
                                user.IsAttention = true;
                                continue;
                            }
                        }
                        if (!attentionList.Contains(user.ID))
                            user.IsAttention = false;
                    }
                }
                else
                {
                    foreach (UserViewModel child in tempAttentions)
                    {
                        UserModel user = child.Model as UserModel;
                        user.IsAttention = false;
                    }
                }

                UpdateGroupBy();
            }));
        }

        public void UpdateGroupBy()
        {
            var cv = (ListCollectionView)CollectionViewSource.GetDefaultView(this.Items);
            if (cv == null)
            {
                return;
            }

            cv.SortDescriptions.Clear();
            cv.SortDescriptions.Add(new SortDescription("FirstChar", ListSortDirection.Ascending));
            cv.SortDescriptions.Add(new SortDescription("Model.DisplayName", ListSortDirection.Ascending));

            cv.GroupDescriptions.Clear();
            cv.GroupDescriptions.Add(new PropertyGroupDescription("GroupByChar"));
        }
    }
}
