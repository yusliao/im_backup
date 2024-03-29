﻿using SDKClient.Model;
using SDKClient.WebAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Util;

namespace CSClient.ViewModels
{
    public class HistoryChatListViewModel : ListViewModel<HistoryChat>
    {
        System.Collections.Concurrent.ConcurrentDictionary<int, List<HistoryChat>> accountDic = new System.Collections.Concurrent.ConcurrentDictionary<int, List<HistoryChat>>();
        protected override IEnumerable<HistoryChat> GetSearchResult(string key)
        {

            if (string.IsNullOrEmpty(key) || key.Length < 11)
            {
                return null;
            }
            var r = SDKClient.SDKClient.Instance.QueryuserByMobile(key);

            if (r.success)
            {
                var item = r.entity;
               
                IMModels.HistoryChatItem chat = new IMModels.HistoryChatItem();
                chat.imOpendId = item.userId.ToString();
                chat.mobile = item.mobile;
                chat.servicersName = item.sessionId;
                chat.sessionId = item.sessionId;
                chat.sessionType = item.sessionType;
                chat.DisplayName = item.shopName;
                chat.HeadImg = item.photo;
                chat.EndDate = "";
                chat.ID = item.userId;
                ObservableCollection<HistoryChat> list = new ObservableCollection<HistoryChat>();
               
              
                HistoryChat chatVM = new HistoryChat(chat);
                list.Add(chatVM);
                
                return list;
            }
            else
                return null;
        }
        public HistoryChatListViewModel(IListView view) : base(view)
        {
            Getuserhistorylist();
            PageIndex = 1;
            IsFristPageEnabled = false;
        }

        private HistoryChat _selectedItem;
        /// <summary>
        /// 当前选项
        /// </summary>
        public override HistoryChat SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                this.PriorSelectedItem = _selectedItem;

                if (_selectedItem != value)
                {


                    _selectedItem = value;
                    if (_selectedItem != null)
                    {

                        //_selectedItem.Acitve();
                        _selectedItem.View = View.GetItemView(value);
                        //if (_selectedItem.View == null)
                        //{
                        //    _selectedItem.View = View.GetItemView(value);
                        //}

                    }
                }

                if (AppData.MainMV.ListViewModel != this)
                {
                    AppData.MainMV.ListViewModel = this;
                }


                AppData.MainMV.ChangeTrayWindowSize();

                App.Current.MainWindow.Activate();

                this.OnPropertyChanged();
            }
        }
        int _pageIndex;
        public int PageIndex
        {
            get => _pageIndex;
            set
            {
                _pageIndex = value;
                this.OnPropertyChanged();
            }
        }
        bool _IsFristPageEnabled;
        public bool IsFristPageEnabled { get => _IsFristPageEnabled; set { _IsFristPageEnabled = value; this.OnPropertyChanged(); } }
        bool _IsLastPageEnabled;
        public bool IsLastPageEnabled { get => _IsLastPageEnabled; set { _IsLastPageEnabled = value; this.OnPropertyChanged(); } }
        int _totalCount;
        public int TotalCount { get => _totalCount; set { _totalCount = value; this.OnPropertyChanged(); } }


        private async void Getuserhistorylist(int pageIndex = 1, int queryType = 0)
        {
            try
            {
                await SDKClient.SDKClient.Instance.Getuserhistorylist(pageIndex, queryType).ContinueWith(t =>
                {
                    if (t.IsFaulted || t.Result == null)
                        return;
                    HistoryRecordListResp listResp = t.Result;
                    this.PageIndex = listResp.data.pageIndex;
                    this.TotalCount = listResp.data.pageCount;
                    this.IsFristPageEnabled = this.PageIndex == 1 ? false : true;
                    this.IsLastPageEnabled = this.PageIndex == this.TotalCount ? false : true;
                    foreach (HistoryRecordListResp.Data.csUser item in listResp.data.list)
                    {
                        IMModels.HistoryChatItem chat = new IMModels.HistoryChatItem();
                        chat.imOpendId = item.imOpenId;
                        chat.mobile = item.mobile;
                        chat.servicersName = item.servicersName;
                        chat.sessionId = item.sessionId;
                        chat.sessionType = item.sessionType;
                        chat.DisplayName = item.userName;
                        chat.HeadImg = item.userPhoto;
                        chat.EndDate = item.endDate;
                        chat.ID = item.imOpenId.ToInt();

                        HistoryChat chatVM = this.Items.FirstOrDefault(info => info.ID == chat.ID);
                        if (chatVM == null)
                        {
                            chatVM = new HistoryChat(chat);
                            Items.Add(chatVM);
                        }
                    }
                    accountDic.GetOrAdd(pageIndex, this.Items.ToList());

                }, View.UItaskScheduler);
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        #region 命令
        public VMCommand JumpToChatCommand
        {
            get
            {
                return new VMCommand(JumpToChat);
            }
        }
        public VMCommand PageIndexKeyEventCommand
        {
            get
            {
                return new VMCommand(GetCurrentPageData);
            }
        }

        private void GetCurrentPageData(object obj)
        {
            if (_pageIndex == 1)
            {
                IsFristPageEnabled = false;
            }
            if (_pageIndex == TotalCount)
                IsLastPageEnabled = false;
            List<HistoryChat> lst = null;
            if (accountDic.TryGetValue(_pageIndex, out lst))
            {
                this.Items.Clear();
                lst.ForEach(c => this.Items.Add(c));
            }
            else
            {
                this.Items.Clear();
                Getuserhistorylist(_pageIndex);
            }
        }

        private void JumpToChat(object obj)
        {

        }
        /// <summary>
        /// 上一页命令
        /// </summary>
        public VMCommand PreviousPageCommand
        {
            get
            {
                return new VMCommand(PreviousPage);
            }
        }
        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="obj"></param>
        private void PreviousPage(object obj)
        {
            this.IsLastPageEnabled = true;
            _pageIndex -= 1;

            if (_pageIndex < 2)
            {
                IsFristPageEnabled = false;
                _pageIndex = 1;
            }
            List<HistoryChat> lst = null;
            if (accountDic.TryGetValue(_pageIndex, out lst))
            {
                this.Items.Clear();
                lst.ForEach(c => this.Items.Add(c));
                PageIndex = _pageIndex;
            }
            else
            {
                this.Items.Clear();
                Getuserhistorylist(_pageIndex);

            }
            if (accountDic.Keys.Count > 100)
                accountDic.Clear();

        }

        /// <summary>
        /// 下一页命令
        /// </summary>
        public VMCommand NextPageCommand
        {
            get { return new VMCommand(NextPage); }
        }
        private void NextPage(object obj)
        {
            this.IsFristPageEnabled = true;
            _pageIndex += 1;
            if (_pageIndex > this.TotalCount)
            {
                IsLastPageEnabled = false;
                this.PageIndex = this.TotalCount;
            }
            List<HistoryChat> lst = null;
            if (accountDic.TryGetValue(_pageIndex, out lst))
            {
                this.Items.Clear();
                lst.ForEach(c => this.Items.Add(c));
                PageIndex = _pageIndex;
            }
            else
            {
                this.Items.Clear();
                Getuserhistorylist(_pageIndex);

            }
            if (accountDic.Keys.Count > 100)
                accountDic.Clear();
        }

        /// <summary>
        ///跳转到首页命令
        /// </summary>
        public VMCommand FristPageCommad { get { return new VMCommand(FristPage); } }

        private void FristPage(object obj)
        {
            List<HistoryChat> lst = null;
            if (accountDic.TryGetValue(1, out lst))
            {
                this.Items.Clear();
                lst.ForEach(c => this.Items.Add(c));
                PageIndex = 1;
            }
            else
            {
                this.Items.Clear();
                Getuserhistorylist();

            }
            IsFristPageEnabled = false;
            IsLastPageEnabled = true;
        }

        /// <summary>
        /// 跳转到末页命令
        /// </summary>
        public VMCommand LastPageCommad { get { return new VMCommand(LastPage); } }
        private void LastPage(object obj)
        {
            List<HistoryChat> lst = null;
            if (accountDic.TryGetValue(this.TotalCount, out lst))
            {
                this.Items.Clear();
                lst.ForEach(c => this.Items.Add(c));
                PageIndex = this.TotalCount;
            }
            else
            {
                this.Items.Clear();
                Getuserhistorylist(this.TotalCount);
            }
            IsLastPageEnabled = false;
            IsFristPageEnabled = true;
        }
        #endregion
        public void CustomService(CustomServicePackage package)
        {
            if (package.data.type == (int)SDKClient.SDKProperty.customOption.conn)//结束
            {
                HistoryChat chatVM = this.Items.FirstOrDefault(info => info.ID == package.from.ToInt());
                //没有条目
                if (chatVM == null)
                {
                    return;
                }
                else
                {
                    chatVM.View = null;
                }

               
            }
        }
    }
}
