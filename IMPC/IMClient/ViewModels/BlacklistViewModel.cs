using IMModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.ViewModels
{
    public class BlacklistViewModel : ViewModel
    {
        private ObservableCollection<UserViewModel> _items;

        public ObservableCollection<UserViewModel> Items
        {
            get { return _items; }
            set { _items = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// 查找用户列表View
        /// </summary>
        /// <param name="view"></param>
        public BlacklistViewModel(BlacklistModel model) : base(model)
        {
            this.Items = new ObservableCollection<UserViewModel>();
        }

        public void LoadData(SDKClient.Model.GetBlackListPackage pg)
        {
            if (pg == null || pg.data == null || pg.data.items == null)
            {
                return;
            }
            App.Current.Dispatcher.Invoke(() => {
                this.Items.Clear();
            });
            foreach (var item in pg.data.items)
            {
                UserModel user = AppData.Current.GetUserModel(item.userId);
                user.Name = user.NickName = item.userName;
                user.DisplayName = string.IsNullOrEmpty(user.NickName) ? user.Name : user.NickName;
                user.HeadImg = IMClient.Helper.ImageHelper.GetFriendFace(item.photo, (s) => user.HeadImg = s);
                user.IsDefriend = true;
                user.LinkType = 1;
                AddBlacklistItem(user);
            }
        }

        public void AddBlacklistItem(UserModel user)
        {
            if (this.Items.Any(info => info.ID == user.ID))
            {
                return;
            }

            UserViewModel userVM = new UserViewModel(user);
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                AppData.MainMV.BlacklistVM.Items.Add(userVM);
            }));
        }

        public void UpdateFriendRelation(int id, int linkType)
        {
            SDKClient.SDKClient.Instance.UpdateFriendRelation(linkType, id);
        }

        public void DeleteBlacklistItem(UserModel user)
        {
            UserViewModel userVM = this.Items.FirstOrDefault(info => info.ID == user.ID);
            if(userVM == null)
            {
                return;
            }
            
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                AppData.MainMV.BlacklistVM.Items.Remove(userVM);
            }));
        }
    }

    public class BlacklistModel : BaseModel
    {

    }
}
