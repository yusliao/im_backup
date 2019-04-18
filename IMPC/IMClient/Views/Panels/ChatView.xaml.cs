using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IMClient.ViewModels;
using IMClient.Views.ChildWindows;
using IMModels;
using IMClient.Helper;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using IMClient.Views.Controls;
using System.Windows.Controls.Primitives;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// 聊天框View
    /// </summary>
    public partial class ChatView : UserControl, IView
    {
        const double _appendWidth = 288;
        /// <summary>
        /// 聊天框View
        /// </summary>
        public ChatView(ViewModel vm)
        {
            InitializeComponent();

            this.DataContext = this.ViewModel = vm;

            if (vm is ChatViewModel cvm)
            {
                if (!cvm.IsGroup)
                {
                    this.gridLayout.Children.Remove(this.tbMemberCount);
                }
                cvm.AppendMessage += (m) => {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        this.chatBox.ScallToCurrent(m);
                    });
                };
                cvm.GotViewIsFocus += Cvm_GotViewIsFocus;
                cvm.OnDisplayAtButton += Cvm_OnDisplayAtButton;
                cvm.OnPopUpNoticeWindow += Cvm_OnPopUpNoticeWindow;
                GroupViewModel groupVM = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(info => info.ID == cvm.ID);
                if (cvm.IsGroup && groupVM != null && (groupVM.IsCreator || groupVM.IsAdmin))
                {
                    this.btnApply.DataContext = groupVM;
                    //this.bdApply.Child = new GroupApplyListView() { DataContext = groupVM };
                }
                else
                {
                    this.btnApply.Visibility = Visibility.Collapsed;
                }
            }

            ////确保附加面板关闭时，关闭子面板
            //this.ppAppend.Closed += delegate { AppData.MainMV.IsOpenBusinessCard = false; };
            //this.ppAppend.PreviewMouseDown += delegate { AppData.MainMV.IsOpenBusinessCard = false; };

            this.KeyDown += ChatView_KeyDown;
            this.chatBox.OnHideHistoryMsgButton += ChatBox_OnHideHistoryMsgButton;
            this.chatBox.OnDisplayAtSomeone += ChatBox_OnDisplayAtSomeone;
            this.chatBox.OnOpenHistoryMsg += ChatBox_OnOpenHistoryMsg;
            this.msgEditor.GotDataContext += MsgEditor_GotDataContext;
            this.Unloaded += ChatView_Unloaded;
        }

       

        private void ChatView_Unloaded(object sender, RoutedEventArgs e)
        {
            (AppData.MainMV.View as MainWindow).AppendWindowView?.Close();
        }

        private bool Cvm_GotViewIsFocus()
        {
            if (AppData.MainMV.IsOpenedAppendWindow)
            {
                return true;
            }

            if (App.Current.MainWindow.WindowState == WindowState.Minimized)
            {
                return this.IsFocused;
            }
            else
            {
                return App.Current.MainWindow.IsActive;
            }
        }

        private void Cvm_OnDisplayAtButton()
        {
            if (this.DataContext is ChatViewModel cvm)
            {
                int count = cvm.AtMeDic.Count;
                for (int i = 0; i < count; i++)
                {
                    MessageModel msgModel = cvm.AtMeDic.ElementAt(i).Value;

                    bool b = this.chatBox.IsItemInVisualArea(msgModel);
                    if (b)
                    {
                        cvm.AtMeDic.TryRemove(msgModel.MsgKey, out msgModel);
                        count--;
                        i--;
                    }
                }
                if (cvm.AtMeDic.Count == 0)
                {
                    cvm.IsDisplayAtButton = false;
                }
                else
                {
                    cvm.IsDisplayAtButton = true;
                    cvm.IsDisplayHistoryMsgButton = false;
                }
            }
        }
        private void Cvm_OnPopUpNoticeWindow()
        {
            if (this.DataContext is ChatViewModel cvm)
            {
                if(cvm.IsGroup)
                {
                    if(cvm.currentUnreadGroupNoticeMessage!=null)
                    {
                        if (cvm.IsHasPopup)
                            return;
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if (AppData.tipWindow != null)
                            {
                                AppData.tipWindow.Close();
                                Views.ChildWindows.GroupNoticeTipWindow win = new GroupNoticeTipWindow(cvm.currentUnreadGroupNoticeMessage, true);
                                win.Owner = App.Current.MainWindow;
                                win.ShowDialog();
                            }
                            else
                                AppData.tipWindow = new Views.ChildWindows.GroupNoticeTipWindow(cvm.currentUnreadGroupNoticeMessage);
                        }));
                    }    
                }     
            }
        }

        private ChatViewModel MsgEditor_GotDataContext()
        {
            return this.DataContext as ChatViewModel;
        }

        private void ChatBox_OnDisplayAtSomeone(IChat iChat)
        {
            this.msgEditor.AtSomeoneFromChatBox(iChat);
        }

        private void ChatBox_OnHideHistoryMsgButton()
        {
            ChatViewModel vm = this.DataContext as ChatViewModel;
            vm.IsDisplayHistoryMsgButton = false;
            vm.UnReadMsgTip = string.Empty;
        }

        private void ChatView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter) &&
                Keyboard.Modifiers == ModifierKeys.Shift &&
                (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                //Shift+Enter 换行
                if (e.Key == Key.Enter)
                {
                    Newline();
                    return;
                }
            }
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter) &&
                Keyboard.Modifiers == ModifierKeys.Alt &&
                (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                //Alt+Enter 换行
                Newline();
                return;
            }
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Enter) &&
            Keyboard.Modifiers == ModifierKeys.Control &&
            (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                //Ctrl+Enter 换行
                Newline();
                return;
            }

            if (e.KeyStates == Keyboard.GetKeyStates(Key.S) &&
                Keyboard.Modifiers == ModifierKeys.Alt &&
                (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                Send();
            }
            if (e.Key == Key.Enter)
            {
                Send();
            }
        }

        private void Newline()
        {
            if (msgEditor.CurrentNumberOfCharacters >= 1998)
            {
                return;
            }
            msgEditor.IsNewline = true;
            msgEditor.richBox.Focus();
            msgEditor.richBox.CaretPosition = msgEditor.richBox.CaretPosition.InsertParagraphBreak();
        }

        private void Send()
        {
            if (this.ViewModel is ChatViewModel vm)
            {
                vm.SendCommand.Execute(this.msgEditor.Document);
            }
        }

        public ViewModel ViewModel { get; private set; }

        private void ptbtnAppend_Click(object sender, RoutedEventArgs e)
        {
            if (this.ptbtnAppend.IsChecked == true && this.ViewModel is ChatViewModel vm)
            {
                object content = null;
                if (vm.IsGroup)
                {
                    if (AppData.MainMV.GroupListVM.Items.ToList().Any(info => info.ID == vm.ID))
                    {
                        vm.TargetVM = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(info => info.ID == vm.ID);
                        content = new SetupGroupView() { DataContext = vm };
                    }
                    else
                    {
                        vm.AddMessageTip("该群组已经被解散或您已被管理员请出群组！");
                        return;
                    }
                    var groupVM = AppData.MainMV.GroupListVM.Items.FirstOrDefault(info => info.ID == vm.ID);
                    if (groupVM != null && (groupVM.ShowMembers == null || groupVM.ShowMembers.Count == 0))
                    {
                        groupVM?.GetGroupMemberList();
                    }
                }
                else
                {
                    vm.TargetVM = AppData.MainMV.GroupListVM.Items.FirstOrDefault(info => info.ID == vm.ID);
                    content = new SetupFriendView() { DataContext = vm };
                }
                this.ShowAppend(content, new Action(() => { this.ptbtnAppend.IsChecked = false; }));
            }
            else
            {
                this.ptbtnAppend.IsChecked = false;
                (AppData.MainMV.View as MainWindow).AppendWindowView?.Close();
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (this.btnApply.IsChecked == true && this.ViewModel is ChatViewModel vm && vm.IsGroup)
            {
                object content = new GroupApplyListView() { DataContext = this.btnApply.DataContext };
                this.ShowAppend(content, new Action(() => { this.btnApply.IsChecked = false; }));
            }
            else
            {
                this.btnApply.IsChecked = false;
                (AppData.MainMV.View as MainWindow).AppendWindowView?.Close();
            }
        }

        private void ChatBox_OnOpenHistoryMsg(ToggleButton btn)
        {
            if (btn.IsChecked == true && this.ViewModel is ChatViewModel chatVM && chatVM.Model is ChatModel chat)
            {
                object content = new ChatHistoryView(chat);
                this.ShowAppend(content, new Action(() => { btn.IsChecked = false; }));
            }
            else
            {
                btn.IsChecked = false;
                (AppData.MainMV.View as MainWindow).AppendWindowView?.Close();
            }
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            ChatBox_OnOpenHistoryMsg(this.btnHistory);
        }
        
        private void ShowAppend(object content, Action closed, double width = _appendWidth)
        {
            Point p = this.rectTemp.PointToScreen(new Point(0, 0));
            p.X /= Helper.PrimaryScreen.DpiXRate;
            p.Y /= Helper.PrimaryScreen.DpiYRate;

            bool isInner = false;
            double height = this.rectTemp.ActualHeight;
            if (Helper.PrimaryScreen.WorkingArea.Width / Helper.PrimaryScreen.DpiXRate - p.X < width)
            {
                p.X -= width;
                p.Y += 60;
                isInner = true;
                height -= 60;
            }

            Rect rect = new Rect(p.X, p.Y, width, height);

            //AppendWindow win = new AppendWindow(rect, isInner, content);
            (AppData.MainMV.View as MainWindow).AppendWindowView?.Close();
            (AppData.MainMV.View as MainWindow).AppendWindowView = new AppendWindow(rect, isInner, content);
            AppData.MainMV.IsOpenedAppendWindow = true;

            (AppData.MainMV.View as MainWindow).AppendWindowView.Closed += delegate
            {
                AppData.MainMV.IsOpenedAppendWindow = false;
                Task.Delay(1).ContinueWith(task =>
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        closed?.Invoke();
                    }));
                });
            };
            (AppData.MainMV.View as MainWindow).AppendWindowView.Show();
        }

        private void btnHistoryMsg_Click(object sender, RoutedEventArgs e)
        {
            ChatViewModel vm = this.DataContext as ChatViewModel;
            int unreadCount = 0;
            if(!string.IsNullOrEmpty(vm.UnReadMsgTip))
            {
                var strArr = vm.UnReadMsgTip.Split(new string[] { "条消息" }, StringSplitOptions.RemoveEmptyEntries);
                int.TryParse(strArr[0], out unreadCount);
                //unreadCount = int.Parse(strArr[0]);
            }
            vm.IsDisplayHistoryMsgButton = false;
            ChatViewModel chatVm = AppData.MainMV.ChatListVM.Items.ToList().FirstOrDefault(x => x.ID == vm.ID);
            vm.UnReadMsgTip = string.Empty;
            MessageModel chatmsg = (vm.Model as ChatModel).Messages.LastOrDefault(x => x.MsgType == MessageType.notification && x.Content == ConstString.FollowingIsNewMessage);
            if (chatmsg != null)
            {
                this.chatBox.ScallToCurrent(chatmsg);
            }
            else
            {
                var chatMsgList = (vm.Model as ChatModel).Messages.OrderByDescending(x => x.SendTime).Take(unreadCount).OrderByDescending(x=>x.SendTime);
                MessageModel msgmodel = chatMsgList.Last();
                this.chatBox.ScallToCurrent(msgmodel);
                //this.chatBox.ScallToHome();//.ScallToCurrent((vm.Model as ChatModel).Messages[0]);
            }
        }

        private void btnAt_Click(object sender, RoutedEventArgs e)
        {
            ChatViewModel vm = this.DataContext as ChatViewModel;

            MessageModel chatmsg = vm.AtMeDic.First().Value;
            MessageModel targetMsg = (vm.Model as ChatModel).Messages.FirstOrDefault(x => x.MsgKey == chatmsg.MsgKey);

            if (targetMsg != null)
            {
                this.chatBox.ScallToCurrent(targetMsg);
                //vm.AtMeDic.TryRemove(chatmsg.MsgKey, out chatmsg);                
            }
            vm.AtMeDic.Clear();
            vm.IsDisplayAtButton = false;
        }

        private void ptbtnAppendGroupNotice_Click(object sender, RoutedEventArgs e)
        {
            if (this.ptbtnAppendGroupNotice.IsChecked == true && this.ViewModel is ChatViewModel vm)
            {
                object content = null;
                if (vm.IsGroup)
                {
                    if (AppData.MainMV.GroupListVM.Items.ToList().Any(info => info.ID == vm.ID))
                    {

                        vm.TargetVM = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(info => info.ID == vm.ID);
                        GroupViewModel gvm = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(info => info.ID == vm.ID);
                        content = new GroupNoticeView() { DataContext = vm };
                    }
                    else
                    {
                        vm.AddMessageTip("该群组已经被解散或您已被管理员请出群组！");
                        return;
                    }
                    //var groupVM = AppData.MainMV.GroupListVM.Items.FirstOrDefault(info => info.ID == vm.ID);
                    //if (groupVM != null && groupVM.ShowMembers.Count == 0)
                    //{
                    //    groupVM?.GetGroupMemberList();
                    //}
                }
                else
                {
                    return;
                }
                this.ShowAppend(content, new Action(() => { this.ptbtnAppendGroupNotice.IsChecked = false; }));
            }
            else
            {
                this.ptbtnAppendGroupNotice.IsChecked = false;
                (AppData.MainMV.View as MainWindow).AppendWindowView?.Close();
            }
        }


    }
}
