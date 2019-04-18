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
    public class ChatListViewModel : ListViewModel<ChatViewModel>
    {
        public event Action<bool, bool> OnFlashIcon;
        public event Action OnCloseTrayWindow;
        public event Func<bool> GotWindowIsActive;

        /// <summary>
        /// 消息列表VM
        /// </summary>
        /// <param name="view"></param>
        public ChatListViewModel(IListView view) : base(view)
        {
            LoadCustomServiceHistoryChats();
            this.Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems.Count == 1 && e.NewItems[0] is ChatViewModel chat)
            {
                var item = this.Items.Count(info => info.ID == chat.ID);
                if (item > 1)
                {
                    App.Logger.Error("添加了重复聊天条目！");
                }
            }
        }

        protected override IEnumerable<ChatViewModel> GetSearchResult(string key)
        {
            if (string.IsNullOrEmpty(key) || key.Length < 11)
            {
                return null;
            }

            var result = SDKClient.SDKClient.Instance.GetUserInfoByMobile(key);
            if (result.success)
            {
                if (result.entity != null)
                {
                    IChat chat = new UserModel()
                    {
                        ID = result.entity.userId,
                        DisplayName = result.entity.mobile,
                        HeadImg = ImagePathHelper.DefaultUserHead,
                    };

                    chat.HeadImg = result.entity.photo ?? ImagePathHelper.DefaultUserHead;
                    ChatModel chatModel = AppData.Current.GetChatViewModel(chat);
                   
                    ChatViewModel chatVM = new ChatViewModel(chatModel);
                    CustomUserModel customUserModel = new CustomUserModel()
                    {
                        AppType = result.entity.appType,
                        ShopBackUrl = result.entity.shopBackUrl,
                        ShopId = result.entity.shopId,
                        ShopName = result.entity.shopName ?? result.entity.mobile,
                        Mobile = result.entity.mobile
                    };
                    chatVM.CustomUserModel = customUserModel;
                    chatVM.SessionId = result.entity.sessionId;
                    if (result.entity.sessionType == 0)
                    {
                        chatVM.StartOrStopSession(false);
                        chatVM.sessionType = 0;
                    }
                    else if (result.entity.sessionType == 1)
                    {
                        chatVM.StartOrStopSession(true);
                        chatVM.SessionId = result.entity.sessionId;
                        chatVM.sessionType = 1;
                    }
                    else
                    {
                        chatVM.sessionType = 2;//别人的会话
                    }
                    ObservableCollection<ChatViewModel> list = new ObservableCollection<ChatViewModel>();
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        list.Add(chatVM);
                    }));
                    return list;
                }
            }
            return this.Items.Where(info => !string.IsNullOrEmpty((info.Model as ChatModel).Chat.DisplayName) && (info.Model as ChatModel).Chat.DisplayName.Equals(key));
        }

        #region Propertys

        private ChatViewModel _selectedItem;
        /// <summary>
        /// 当前选项
        /// </summary>
        public override ChatViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                this.PriorSelectedItem = _selectedItem;

                if (_selectedItem != value)
                {
                    if (_selectedItem != null)
                    {
                        _selectedItem.AtMeDic.Clear();
                    }

                    _selectedItem = value;
                    if (_selectedItem != null)
                    {
                        _selectedItem.Acitve();

                        if (_selectedItem.View == null)
                        {
                            _selectedItem.View = View.GetItemView(value);
                        }
                    }
                }

                if (AppData.MainMV.ListViewModel != this)
                {
                    AppData.MainMV.ListViewModel = this;
                }

                AppData.MainMV.UpdateUnReadMsgCount();
                TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
                if (TotalUnReadCount == 0)
                {
                    this.OnCloseTrayWindow?.Invoke();
                }
                AppData.MainMV.ChangeTrayWindowSize();

                App.Current.MainWindow.Activate();

                this.OnPropertyChanged();
            }
        }

        private int _unReadCount;
        /// <summary>
        /// 未读消息数总量
        /// </summary>
        public int TotalUnReadCount
        {
            get
            {
                return _unReadCount;
            }
            set
            {
                _unReadCount = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        public void CloseTrayWindow()
        {
            this.OnCloseTrayWindow?.Invoke();
        }

        public void IgnoreAllNewMessages()
        {
            foreach (var item in this.Items)
            {
                if (item.UnReadCount > 0 && !item.Chat.Chat.IsNotDisturb)
                {
                    item.Acitve();
                }
            }

            AppData.MainMV.UpdateUnReadMsgCount();
            TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
            this.OnFlashIcon?.Invoke(false, false);
        }

        private async void LoadCustomServiceHistoryChats()
        {
            
            await SDKClient.SDKClient.Instance.GetCSRoomlist().ContinueWith(t =>
            {
               
                var chats = t.Result;
                foreach (var item in chats)
                {
                    IChat chat = AppData.Current.GetUserModel(item.userId);
                    chat.DisplayName = item.shopName;
                    
                  
                    chat.HeadImg = item.photo ?? ImagePathHelper.DefaultUserHead;

                    ChatViewModel chatVM = this.Items.FirstOrDefault(info => info.ID == chat.ID);
                    if (chatVM == null)
                    {
                        var last = new MessageModel()
                        {
                            Sender = chat,
                        };
                        ChatModel model = AppData.Current.GetChatViewModel(chat);
                        chatVM = new ChatViewModel(model, last);
                        CustomUserModel customUserModel = new CustomUserModel()
                        {
                            AppType = item.appType,
                            ShopBackUrl = item.shopBackUrl,
                            ShopId = item.shopId,
                            ShopName = item.shopName ?? item.mobile,
                            Mobile = item.mobile
                        };
                        chatVM.CustomUserModel = customUserModel;
                        chatVM.SessionId = item.sessionId;
                        if (item.sessionType == 0)
                        {
                            chatVM.StartOrStopSession(false);
                            chatVM.sessionType = 0;
                        }
                        else if (item.sessionType == 1)
                        {
                            chatVM.StartOrStopSession(true);
                            chatVM.SessionId = item.sessionId;
                            chatVM.sessionType = 1;
                        }
                        else
                        {
                            chatVM.sessionType = 2;//别人的会话
                        }

                           
                        Items.Add(chatVM);
                           
                    }
                }
                
               
            }, View.UItaskScheduler);
           
        }

        private void AddVirtualChatItem()
        {
            IChat chat = AppData.Current.GetUserModel(88888);
            chat.DisplayName = "13589784621";
            chat.HeadImg = ImagePathHelper.DefaultUserHead;

            ChatViewModel chatVM = this.Items.FirstOrDefault(info => info.ID == chat.ID);
            if (chatVM == null)
            {
                var last = new MessageModel()
                {
                    Sender = chat,
                };
                ChatModel model = AppData.Current.GetChatViewModel(chat);
                chatVM = new ChatViewModel(model, last);
                chatVM.StartOrStopSession(false);
                App.Current.Dispatcher.Invoke(() =>
                {
                    Items.Add(chatVM);
                });
            }
        }

        public void EndSession(SysNotifyPackage package)
        {
            if (package.data.type == 3 && package.data.subType == -100)
            {
                ChatViewModel chatVM = this.Items.FirstOrDefault(info => info.ID == package.from.ToInt());
                if (chatVM != null)
                {
                    chatVM.AddMessageTip("结束聊天");
                    chatVM.IsSessionEnd = true;
                    chatVM.SessionId = string.Empty;
                    chatVM.StartOrStopSession(false);
                    chatVM.sessionType = 0;//重置会话状态为结束状态
                    var result = SDKClient.SDKClient.Instance.SendCustiomServerMsg(chatVM.ID.ToString(), package.id, SDKClient.SDKProperty.customOption.over).Result;
                }
            }
        }

        public void SetSession(CustomServicePackage package)
        {
            if (package.data.type == 3)
                return;
             
            
            if (package.data.type == (int)SDKClient.SDKProperty.customOption.over)//结束
            {
                ChatViewModel chatVM = this.Items.FirstOrDefault(info => info.ID == package.data.originTo.ToInt());
                //没有条目
                if (chatVM == null)
                {
                    return;
                }
                else
                {
                    if (((chatVM.Model as ChatModel).Chat as UserModel).HeadImg.Equals(ImagePathHelper.DefaultUserHead))
                    {
                        ((chatVM.Model as ChatModel).Chat as UserModel).HeadImg = package.data.photo ?? ImagePathHelper.DefaultUserHead;
                    }

                    if (chatVM.Chat.LastMsg != null)
                    {
                        chatVM.Chat.LastMsg.SendTime = package.time.Value;
                    }
                }

                chatVM.AddMessageTip("结束聊天");
                chatVM.IsSessionEnd = true;
                chatVM.SessionId = string.Empty;
                chatVM.StartOrStopSession(false);
                chatVM.sessionType = 0;
                App.Current.Dispatcher.Invoke(() =>
                {
                    AppData.MainMV.ChatListVM.ResetSort();
                });
            }
            else if (package.data.type == (int)SDKClient.SDKProperty.customOption.conn)//接入
            {
                ChatViewModel chatVM = this.Items.FirstOrDefault(info => info.ID == package.from.ToInt());
                if (package.data.originTo == SDKClient.SDKClient.Instance.property.CurrentAccount.userID.ToString())//发送给我的接入
                {
                    //没有条目
                    if (chatVM == null)
                    {
                        IChat chat = new UserModel()
                        {
                            ID = package.from.ToInt(),

                            DisplayName = package.data.shopName??package.data.mobile,
                            HeadImg = package.data.photo ?? ImagePathHelper.DefaultUserHead
                        };
                        

                        ChatModel chatModel = AppData.Current.GetChatViewModel(chat);

                        chatModel.LastMsg = new MessageModel()
                        {
                            SendTime = package.time.Value,
                        };
                        chatVM = new ChatViewModel(chatModel);

                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            this.Items.Add(chatVM);
                        }));
                    }
                    else
                    {
                        if (((chatVM.Model as ChatModel).Chat as UserModel).HeadImg.Equals(ImagePathHelper.DefaultUserHead))
                        {
                            ((chatVM.Model as ChatModel).Chat as UserModel).HeadImg = package.data.photo ?? ImagePathHelper.DefaultUserHead;
                        }

                        if (chatVM.Chat.LastMsg != null)
                        {
                            chatVM.Chat.LastMsg.SendTime = package.time.Value;
                        }


                    }
                    CustomUserModel customUserModel = new CustomUserModel()
                    {
                        AppType = package.data.appType,
                        DeviceName = package.data.deviceName,
                        DeviceType = package.data.deviceType,
                        ShopBackUrl = package.data.shopBackUrl,
                        ShopId = package.data.shopId,
                        ShopName = package.data.shopName??package.data.mobile,
                        //Mobile = package.data.mobile
                    };
                    chatVM.CustomUserModel = customUserModel;
                    chatVM.SessionId = package.data.sessionId;
                    if (!string.IsNullOrEmpty(package.data.address))
                    {
                        Task.Run(async () =>
                        {
                            chatVM.CustomUserModel.Address = await SDKClient.SDKClient.Instance.GetAddressByIP(package.data.address).ConfigureAwait(false);
                        });
                    }

                    chatVM.IsDisplayStartSession = true;
                    chatVM.sessionType = 1;
                    //if((chatVM.Model as ChatModel).LastMsg?.Content!= "您好!欢迎光临满金店,请问有什么可以帮到您？")
                    //    chatVM.SendTextMsgToServer("您好!欢迎光临满金店,请问有什么可以帮到您？");
                    chatVM.StartOrStopSession(true);
                    FlashIcon(chatVM, false);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AppData.MainMV.ChatListVM.ResetSort();
                    });
                }
                else
                {
                    if (chatVM == null)
                    {
                        return;
                    }
                    else
                    {
                        if (((chatVM.Model as ChatModel).Chat as UserModel).HeadImg.Equals(ImagePathHelper.DefaultUserHead))
                        {
                            ((chatVM.Model as ChatModel).Chat as UserModel).HeadImg = package.data.photo ?? ImagePathHelper.DefaultUserHead;
                        }

                        if (chatVM.Chat.LastMsg != null)
                        {
                            chatVM.Chat.LastMsg.SendTime = package.time.Value;
                        }


                    }
                    CustomUserModel customUserModel = new CustomUserModel()
                    {
                        // Address = package.data.address,
                        DeviceName = package.data.deviceName,
                        DeviceType = package.data.deviceType,
                        ShopBackUrl = package.data.shopBackUrl,
                        //Mobile = package.data.mobile,
                        ShopId = package.data.shopId,
                        ShopName = package.data.shopName ?? package.data.mobile,
                    };
                    chatVM.CustomUserModel = customUserModel;
                    chatVM.SessionId = package.data.sessionId;
                    if (!string.IsNullOrEmpty(package.data.address))
                    {
                        Task.Run(async () =>
                        {
                            chatVM.CustomUserModel.Address = await SDKClient.SDKClient.Instance.GetAddressByIP(package.data.address).ConfigureAwait(false);
                        });
                    }

                    chatVM.IsDisplayStartSession = false;
                    chatVM.sessionType = 2;
                  //  chatVM.AddMessageTip("当前用户正在和其他客服进行沟通中", isSetLastMsg: false);
                    chatVM.StartOrStopSession(false);
                  
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        AppData.MainMV.ChatListVM.ResetSort();
                    });
                   
                }
            }
            
        }

        /// <summary>
        /// 加载本地历史消息
        /// </summary>
    

        internal void CustomExchange(CustomExchangePackage package)
        {
            int userid = package.data.originTo.ToInt();//用户ID
            if (package.data.userId != SDKClient.SDKClient.Instance.property.CurrentAccount.userID)
            {
               
                ChatViewModel chatVM = this.Items.FirstOrDefault(info => info.ID == userid);
                if(chatVM!=null)
                {
                    chatVM.AddMessageTip("当前用户正在和其他客服进行沟通中", isSetLastMsg: false);
                    chatVM.sessionType = 2;
                    chatVM.StartOrStopSession(false);        
                }
            }
            else
            {
                
                ChatViewModel chatVM = this.Items.FirstOrDefault(info => info.ID == userid);
                if (chatVM != null)
                {
                    chatVM.sessionType = 1;
                    chatVM.StartOrStopSession(true);

                    //添加通知消息
                    chatVM.AddMessageTip("会话转移接入成功", isSetLastMsg:false);
                }
            }

        }

        /// <summary>
        /// 客服任务回包
        /// </summary>
        /// <param name="package"></param>
        internal void CSSyncMsgStatus(CSSyncMsgStatusPackage package)
        {
            int csid = package.from.ToInt();
            ChatViewModel chatVM = this.Items.FirstOrDefault(info => info.ID == package.data.userId);

            if (csid == SDKClient.SDKClient.Instance.property.CurrentAccount.userID)//发给我的
            {
                //没有条目
                if (chatVM == null)
                {
                    IChat chat = new UserModel()
                    {
                        ID = package.data.userId,

                        DisplayName = package.data.userName,
                        HeadImg = package.data.photo
                    };
                    chat.HeadImg = package.data.photo ?? ImagePathHelper.DefaultUserHead;

                    ChatModel chatModel = AppData.Current.GetChatViewModel(chat);

                    chatModel.LastMsg = new MessageModel()
                    {
                        SendTime = package.time.Value,
                    };
                    chatVM = new ChatViewModel(chatModel);
                    if (package.code == 0)
                        chatVM.sessionType = 0;
                    else
                        chatVM.sessionType = 2;
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        this.Items.Add(chatVM);
                    }));
                }
                else
                {
                    if ((chatVM.Model as ChatModel).Chat.HeadImg.Equals(ImagePathHelper.DefaultUserHead))
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            (chatVM.Model as ChatModel).Chat.HeadImg = package.data.photo ?? ImagePathHelper.DefaultUserHead;
                        }));
                        
                    }

                    if (chatVM.Chat.LastMsg != null)
                    {
                        chatVM.Chat.LastMsg.SendTime = package.time.Value;
                    }
                    if (package.code == 0)
                        chatVM.sessionType = 0;
                    else
                        chatVM.sessionType = 2;
                   
                }
            }
            //else//广播消息
            //{
            //    if (chatVM != null)
            //    {
            //        if (package.code == 0)
            //        {
            //            chatVM.AddMessageTip("当前用户正在和其他客服进行沟通中",isSetLastMsg:false);
            //            chatVM.sessionType = 2;
            //        }
            //    }
            //}
            
           

            
         

        }

     

        /// <summary>
        /// 加载离线消息
        /// </summary>
        /// <param name="datas">消息源</param>
        public void LoadOfflineMsgs(List<dynamic> datas)
        {
            foreach (var d in datas)
            {
                SDKClient.Model.MessagePackage pg = new SDKClient.Model.MessagePackage();

                string value = string.Format("{0}", d);

                value.ToString();
            }
        }

        /// <summary>
        /// 收到新消息
        /// </summary>
        /// <param name="package"></param>
        public void ReceiveMsg(SDKClient.Model.MessagePackage package)
        {
            if (package == null || package.code != 0 || package.data == null)
            {
                //if (package != null && package.code == 500 && package.data.type == "groupChat")
                //{
                //    var vm = AppData.MainMV.GroupListVM.Items.FirstOrDefault(g => g.ID == package.data.groupInfo.groupId);
                //    if (vm != null)
                //    {
                //        var myself = AppData.Current.LoginUser.User.GetInGroupMember((vm.Model as GroupModel));
                //        (vm.Model as GroupModel).Members.Remove(myself);
                //    }
                //}
                return;
            }

            int from = package.from.ToInt();
            int to = package.to.ToInt();
            int chatID = from;

            if (package.data.groupInfo != null)
            {
                chatID = package.data.groupInfo.groupId;
            }
            else
            {
                if (package.syncMsg == 1)
                {
                    chatID = to;
                }
                else
                {
                    chatID = from;
                }
            }

            ChatViewModel chatVM = GetChat(chatID, package.data.groupInfo != null);

            chatVM.ReceiveNewMessage(package, from);

            FlashIcon(chatVM, package.syncMsg == 1 ? true : false);
        }

        public void FlashIcon(ChatViewModel chatVM, bool isSync = false)
        {
            if (isSync)
            {
                return;
            }

            AppData.MainMV.UpdateUnReadMsgCount();
            TotalUnReadCount = AppData.MainMV.TotalUnReadCount;
            AppData.MainMV.ChangeTrayWindowSize();
            //if (this.SelectedItem != null && this.SelectedItem.ID == chatVM.ID)
            //{

            //}
            //else
            //{
            //    if (!chatVM.Chat.Chat.IsNotDisturb)
            //    {
            //        this.OnFlashIcon?.Invoke(true);
            //    }
            //}

            if (!chatVM.Chat.Chat.IsNotDisturb)
            {
                this.OnFlashIcon?.Invoke(true, true);
            }
        }

        public ChatViewModel GetChat(int chatID, bool isGroup = false)
        {
            ChatViewModel chatVM = this.Items.FirstOrDefault(info => info.ID == chatID);

            if (chatVM == null)
            {
                IChat chat;
                if (isGroup)
                {
                    chat = AppData.Current.GetGroupModel(chatID);
                }
                else
                {
                    chat = AppData.Current.GetUserModel(chatID);
                }
                ChatModel chatModel = AppData.Current.GetChatViewModel(chat);

                chatVM = new ChatViewModel(chatModel);
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    this.Items.Add(chatVM);
                }));
            }

            int toomType = isGroup ? 1 : 0;
            SDKClient.SDKClient.Instance.UpdateChatRoomVisibility(chatID, toomType, true);
            return chatVM;
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
    }
}
