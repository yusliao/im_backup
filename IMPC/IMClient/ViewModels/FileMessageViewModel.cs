using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IMModels;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 文件信息VM
    /// </summary>
    public class FileMessageViewModel : ViewModel
    {
        MessageModel _targeModel;
        public FileMessageViewModel(MessageModel model) : base(model)
        {
            if(model==null || model.MsgType != MessageType.file)
            {
                throw new Exception("信息类型MessageType 不符合!");
            }
            _targeModel = model;
        }



        #region Comamnds

        private VMCommand _onlineSendCommand;
        /// <summary>
        /// 在线发送命令
        /// </summary> 
        public VMCommand OnlineSendCommand
        {
            get
            {
                if (_onlineSendCommand == null)
                    _onlineSendCommand = new VMCommand(this.OnlineSend);
                return _onlineSendCommand;
            }
        }

        private VMCommand _offlineSendCommand;
        /// <summary>
        /// 离线发送命令
        /// </summary> 
        public VMCommand OfflineSendCommand
        {
            get
            {
                if (_offlineSendCommand == null)
                    _offlineSendCommand = new VMCommand(this.OfflineSend);
                return _offlineSendCommand;
            }
        }


        private VMCommand _receiveCommand;
        /// <summary>
        /// 接收命令
        /// </summary> 
        public VMCommand ReceiveCommand
        {
            get
            {
                if (_receiveCommand == null)
                    _receiveCommand = new VMCommand(this.Receive);
                return _receiveCommand;
            }
        }


        private VMCommand _openCommand;
        /// <summary>
        /// 打开命令
        /// </summary> 
        public VMCommand OpenCommand
        {
            get
            {
                if (_openCommand == null)
                    _openCommand = new VMCommand(this.Open);
                return _openCommand;
            }
        }

        private VMCommand _saveAsCommand;
        /// <summary>
        /// 另存为命令
        /// </summary> 
        public VMCommand SaveAsCommand
        {
            get
            {
                if (_saveAsCommand == null)
                    _saveAsCommand = new VMCommand(this.SaveAs);
                return _saveAsCommand;
            }
        }


        private VMCommand _deleteCommand;
        /// <summary>
        /// 删除命令
        /// </summary> 
        public VMCommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                    _deleteCommand = new VMCommand(this.Delete);
                return _deleteCommand;
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
                    _cancelCommand = new VMCommand(this.Cancel);
                return _cancelCommand;
            }
        }
        #endregion

        #region CommandOperateMethods

        /// <summary>
        /// 在线发送
        /// </summary>
        private void OnlineSend(object para)
        {

        }

        /// <summary>
        /// 离线发送
        /// </summary>
        private void OfflineSend(object para)
        {

        }


        /// <summary>
        /// 开始接收
        /// </summary>
        private void Receive(object para)
        {

        }

        /// <summary>
        /// 打开 
        /// </summary>
        private void Open(Object para)
        {
            string fullName = $"{ this._targeModel.Content}";
            if (File.Exists(fullName))
            {
                Views.ChildWindows.AppendWindow.AutoClose = false;
                System.Diagnostics.Process.Start(fullName);
                Views.ChildWindows.AppendWindow.AutoClose = true;
            }
            else
            {
                AppData.MainMV.TipMessage = "文件不存在!";
            }
        }

        /// <summary>
        /// 另存为
        /// </summary>
        private void SaveAs(Object para)
        { 
            string fullName = $"{ this._targeModel.Content}";
            if (File.Exists(fullName))
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = System.IO.Path.GetFileName(fullName); // Default file name
                dlg.DefaultExt = System.IO.Path.GetExtension(fullName);// fileName.Split('.').LastOrDefault(); // Default file extension

                dlg.Filter = string.Format("文件 (.{0})|*.{0}", dlg.DefaultExt);// // Filter files by extension

                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                if (dlg.ShowDialog() == true)
                {
                    File.Copy(fullName, dlg.FileName, true);
                }
            }
            else
            {
                AppData.MainMV.TipMessage = "文件不存在!";
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        private void Delete(object para)
        {
            //没有太好的关联方式，目前暂时做成当前窗口（因为只有当前窗口可能删除对应信息）；
            ChatViewModel chatVM = AppData.MainMV.ChatListVM.SelectedItem;
            if (chatVM != null)
            {
                chatVM.HideMessageCommand.Execute(this._targeModel);
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void Cancel(object para)
        {

        }
        #endregion
    }
}
