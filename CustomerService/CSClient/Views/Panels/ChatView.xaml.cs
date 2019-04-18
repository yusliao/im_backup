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
using CSClient.ViewModels;
using CSClient.Views.ChildWindows;
using IMModels;
using CSClient.Helper;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;

namespace CSClient.Views.Panels
{
    /// <summary>
    /// 聊天框View
    /// </summary>
    public partial class ChatView : UserControl, IView
    {

        const double _appendWidth = 280;
        /// <summary>
        /// 聊天框View
        /// </summary>
        public ChatView(ViewModel vm)
        {
            InitializeComponent();
            UItaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            this.DataContext = this.ViewModel = vm;

            if (vm is ChatViewModel cvm)
            {
                if (!cvm.IsGroup)
                {
                    //this.gridLayout.Children.Remove(this.tbMemberCount);
                }
                cvm.AppendMessage += (m) => { this.chatBox.ScallToCurrent(m); };
                cvm.GotViewIsFocus += Cvm_GotViewIsFocus;
                cvm.OnDisplayAtButton += Cvm_OnDisplayAtButton;
                cvm.OnStartOrStopSession += Cvm_OnStartOrStopSession;
            }

            ////确保附加面板关闭时，关闭子面板
            //this.ppAppend.Closed += delegate { AppData.MainMV.IsOpenBusinessCard = false; };
            //this.ppAppend.PreviewMouseDown += delegate { AppData.MainMV.IsOpenBusinessCard = false; };

            this.KeyDown += ChatView_KeyDown;
            this.chatBox.OnHideHistoryMsgButton += ChatBox_OnHideHistoryMsgButton;
            this.chatBox.OnDisplayAtSomeone += ChatBox_OnDisplayAtSomeone;
            this.msgEditor.GotDataContext += MsgEditor_GotDataContext;

        }

        private void Cvm_OnStartOrStopSession(bool isStart)
        {
            this.msgEditor?.StartOrStopSession(isStart);
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

        public TaskScheduler UItaskScheduler { get; private set; }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            if (this.btnHistory.IsChecked == true && this.ViewModel is ChatViewModel chatVM && chatVM.Model is ChatModel chat)
            {
                object content = new ChatHistoryView(chatVM);
                this.ShowAppend(content, new Action(() => { this.btnHistory.IsChecked = false; }));
            }
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

            AppendWindow win = new AppendWindow(rect, isInner, content);
            AppData.MainMV.IsOpenedAppendWindow = true;

            win.Closed += delegate
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
            win.Show();
        }

        private void btnHistoryMsg_Click(object sender, RoutedEventArgs e)
        {
            ChatViewModel vm = this.DataContext as ChatViewModel;
            vm.IsDisplayHistoryMsgButton = false;
            vm.UnReadMsgTip = string.Empty;

            MessageModel chatmsg = (vm.Model as ChatModel).Messages.FirstOrDefault(x => x.MsgType == MessageType.notification && x.Content == ConstString.FollowingIsNewMessage);
            if (chatmsg != null)
            {
                this.chatBox.ScallToCurrent(chatmsg);
            }
            else
            {
                this.chatBox.ScallToHome();//.ScallToCurrent((vm.Model as ChatModel).Messages[0]);
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
                vm.AtMeDic.Clear();
                vm.IsDisplayAtButton = false;
            }

            //Cvm_OnDisplayAtButton();
        }

        private void ptbtnAppend_Click(object sender, RoutedEventArgs e)
        {
            if (this.ptbtnAppend.IsChecked == true && this.ViewModel is ChatViewModel vm)
            {                 
                object content = new ScanFastReply() { DataContext = AppData.MainMV.SettingListVM };
                
                this.ShowAppend(content, new Action(() => { this.ptbtnAppend.IsChecked = false; }));
            }
            
        }
        
    }
}
