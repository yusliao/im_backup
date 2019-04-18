using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSClient.ViewModels
{

    public interface IListViewModel 
    {
        bool IsChecked { get; set; }
        //IListView View { get; set; }
        //ViewModel SelectedItem { get; set; } 
        //ObservableCollection<ViewModel> Items { get; set; }
        void Search(string key);

        //Visibility SearchVisibility { get; }
    }


    //public abstract class ViewModelCollection<T> : ObservableCollection<T>, IListViewModel  
    //    where T : ViewModel
    //{
    //    private bool _isChecked;
    //    /// <summary>
    //    /// 是否选中
    //    /// </summary>
    //    public bool IsChecked
    //    {
    //        get { return _isChecked; }
    //        set { _isChecked = value; this.OnPropertyChanged(); }
    //    }

    //    public void Search(string key)
    //    { 
    //        this.SearchResults = string.IsNullOrWhiteSpace(key) ? null : GetSearchResult(key);
    //    }

    //    protected abstract IEnumerable<T> GetSearchResult(string key);

         
    //    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    //    {
    //        base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    //    }
    //}



    /// <summary>
    /// 基础ListViewModel
    /// </summary>
    public abstract class ListViewModel<T> : ViewModel, IListViewModel where T : ViewModel
    {
        public ListViewModel(IListView view) : base(view)
        {
            App.Current.Dispatcher.Invoke(() => { Items = new ObservableCollection<T>(); });
            this.View = view;
        }

        private bool _isChecked;
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; this.OnPropertyChanged(); }
        }

        /// <summary>
        /// 对应的View
        /// </summary>
        public new IListView View { get; private set; }

        private ObservableCollection<T> _items;
        /// <summary>
        /// 子项列表
        /// </summary>
        public ObservableCollection<T> Items
        {
            get { return _items; }
            protected set { _items = value; this.OnPropertyChanged(); }
        }
      
        private T _selectedItem;
        /// <summary>
        /// 当前选项
        /// </summary>
        public virtual T SelectedItem   
        {
            get { return _selectedItem; }
            set
            {
                PriorSelectedItem = _selectedItem;
                _selectedItem = value;
                if (_selectedItem != null && _selectedItem.View == null)
                {
                    _selectedItem.View = View.GetItemView(value);

                }
                this.OnPropertyChanged();
            }
        }

        private T _priorSelectedItem;
        //前一个选中的项目
        public T PriorSelectedItem
        {
            get { return _priorSelectedItem; }
            protected set { _priorSelectedItem = value; this.OnPropertyChanged(); } 
        }

        private T _selectedSearchItem;
        /// <summary>
        /// 当前选项
        /// </summary>
        public virtual T SelectedSearchItem
        {
            get { return _selectedSearchItem; }
            set
            {
                _selectedSearchItem = value; this.OnPropertyChanged();
                if (_selectedSearchItem != null)
                {
                    this.SelectedItem = _selectedSearchItem;
                }
            }
        }

        private Visibility _searchVisibility = Visibility.Collapsed;
        /// <summary>
        /// 搜索结果面板是否可见
        /// </summary>
        public Visibility SearchVisibility
        {
            get { return _searchVisibility; }
            private set { _searchVisibility = value; this.OnPropertyChanged(); }
        }

        private IEnumerable<T> _searchResults;
        /// <summary>
        /// 搜索结果
        /// </summary>
        public IEnumerable<T> SearchResults
        {
            get { return _searchResults; }
            private set
            {
                _searchResults = value;
                this.OnPropertyChanged();

                if (_searchResults == null)
                {
                    this.SearchVisibility = Visibility.Collapsed;
                }
                else
                {
                    this.SearchVisibility = Visibility.Visible;
                }
            }
        }


        public void Search(string key)
        {
            this.SearchResults = string.IsNullOrWhiteSpace(key) ? null : GetSearchResult(key);            
        }

        protected abstract IEnumerable<T> GetSearchResult(string key);
    }
}
