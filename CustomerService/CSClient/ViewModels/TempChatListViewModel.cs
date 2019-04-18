using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using IMModels;
using Util;
using System.ComponentModel;
using System.Windows.Data;
using SDKClient.Model;
using CSClient.Views.ChildWindows;
using CSClient.Helper;

namespace CSClient.ViewModels
{
    /// <summary>
    /// 聊天列表VM
    /// </summary>
    public class TempChatListViewModel : ListViewModel<TempCustomItem>
    {
      
        public event Func<bool> GotWindowIsActive;

        /// <summary>
        /// 消息列表VM
        /// </summary>
        /// <param name="view"></param>
        public TempChatListViewModel(IListView view) : base(view)
        {
            LoadTempCustomServiceHistoryChats();
            this.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems.Count == 1 && e.NewItems[0] is TempCustomItem chat)
            {
                var item = this.Items.Count(info => info.ID == chat.ID);
                if (item > 1)
                {
                    App.Logger.Error("添加了重复聊天条目！");
                }
            }
        }

        protected override IEnumerable<TempCustomItem> GetSearchResult(string key)
        {
            return null;
            //if (string.IsNullOrEmpty(key) || key.Length < 11)
            //{
            //    return null;
            //}

            //var result = SDKClient.SDKClient.Instance.GetUserInfoByMobile(key);
            //if (result.success)
            //{
            //    if (result.entity != null)
            //    {
            //        IChat chat = new UserModel()
            //        {
            //            ID = result.entity.userId,
            //            DisplayName = result.entity.mobile,
            //            HeadImg = ImagePathHelper.DefaultUserHead,
            //        };

            //        chat.HeadImg = result.entity.photo ?? ImagePathHelper.DefaultUserHead;
            //        ChatModel chatModel = AppData.Current.GetChatViewModel(chat);

            //        ChatViewModel chatVM = new ChatViewModel(chatModel);

            //        ObservableCollection<ChatViewModel> list = new ObservableCollection<ChatViewModel>();
            //        App.Current.Dispatcher.Invoke(new Action(() =>
            //        {
            //            list.Add(chatVM);
            //        }));
            //        return list;
            //    }
            //}
            //return this.Items.Where(info => !string.IsNullOrEmpty((info.Model as ChatModel).Chat.DisplayName) && (info.Model as ChatModel).Chat.DisplayName.Equals(key));
        }

        //#region Propertys

        //private TempCustomItem _selectedItem;
        ///// <summary>
        ///// 当前选项
        ///// </summary>
        //public override TempCustomItem SelectedItem
        //{
        //    get { return _selectedItem; }
        //    set
        //    {
        //        this.PriorSelectedItem = _selectedItem;

        //        if (_selectedItem != value)
        //        {
                    

        //            _selectedItem = value;
        //            if (_selectedItem != null)
        //            {
        //               // _selectedItem.Acitve();

        //                if (_selectedItem.View == null)
        //                {
        //                    _selectedItem.View = View.GetItemView(value);
        //                }
        //            }
        //        }

        //        if (AppData.MainMV.ListViewModel != this)
        //        {
        //            AppData.MainMV.ListViewModel = this;
        //        }

             
        //        AppData.MainMV.ChangeTrayWindowSize();

        //       // App.Current.MainWindow.Activate();

        //        this.OnPropertyChanged();
        //    }
        //}
       

        //#endregion

        private async void LoadTempCustomServiceHistoryChats()
        {
            
            await SDKClient.SDKClient.Instance.GetTempCSRoomlist().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    return;
                var chats = t.Result;
                foreach (var item in chats)
                {
                    CustomItem chat = new CustomItem();
                    chat.DisplayName = item.userName;
                    chat.HeadImg = item.photo ?? ImagePathHelper.DefaultUserHead;
                    chat.ID = item.userId;
                    
                    TempCustomItem chatVM = this.Items.FirstOrDefault(info => info.ID == chat.ID);
                    if (chatVM == null)
                    {
                        var last = new MessageModel()
                        {
                            Sender = chat,
                            Content = item.message,
                            SendTime = item.msgTime,
                            
                        };
                        
                        chatVM = new TempCustomItem(chat, last);
                        chatVM.UnReadCount = item.UnreadCount;
                           
                        Items.Add(chatVM);
                           
                    }
                }
               
            }, View.UItaskScheduler);
            await Task.Run(() =>
            {
                var userIds = Items.Select(t => t.ID).ToArray();
                
                return SDKClient.SDKClient.Instance.Postbaseinfo(userIds.Select(i=>i.ToString()).ToArray());
                
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    return;
                foreach (SDKClient.DTO.baseInfoEntity item in t.Result)
                {
                    TempCustomItem chatVM = this.Items.FirstOrDefault(info => info.ID == item.userId);
                    if (chatVM != null)
                    {
                        chatVM.Chat.DisplayName = item.userName;
                        chatVM.Chat.HeadImg = item.photo ?? ImagePathHelper.DefaultUserHead;
                    }
                }
                
            }, View.UItaskScheduler);
        }
        internal void CSSyncMsgStatus(CSSyncMsgStatusPackage package)
        {
            
          
            TempCustomItem chatVM = this.Items.FirstOrDefault(t => t.ID == package.data.userId);
          
            if (chatVM != null)
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.Items.Remove(chatVM);

                }));

            }
           

        }


        /// <summary>
        /// 重置排序
        /// </summary>
        public void ResetSort()
        {
            var my = this.Items.FirstOrDefault(info => info.ID == AppData.Current.LoginUser.ID);
            if (my != null)
            {
                this.Items.Remove(my);
            }
            ICollectionView cv = CollectionViewSource.GetDefaultView(this.Items);
            if (cv == null)
            {
                return;
            }

            cv.SortDescriptions.Clear();
            cv.SortDescriptions.Add(new SortDescription("Model.Chat.TopMostTime", ListSortDirection.Descending));
            cv.SortDescriptions.Add(new SortDescription("Model.LastMsg.SendTime", ListSortDirection.Descending));
        }

        internal void CustomExchange(CustomExchangePackage package)
        {
            int csid = package.data.originTo.ToInt();
            TempCustomItem chatVM = this.Items.FirstOrDefault(t => t.ID == csid);
            
            if (chatVM != null)
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.Items.Remove(chatVM);

                }));

            }
            
        }

        internal void CustomService(CustomServicePackage package)
        {
            
          
            int id = package.from.ToInt();
            TempCustomItem chatVM = this.Items.FirstOrDefault(t => t.ID == id);

            if (chatVM != null)
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.Items.Remove(chatVM);

                }));

            }
            
        }
    }
}
