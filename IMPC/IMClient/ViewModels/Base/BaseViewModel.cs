using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using IMModels;

namespace IMClient.ViewModels
{
    /// <summary>
    /// 基础ViewModel
    /// </summary>
    public abstract class ViewModel :  BaseModel
    {
        public ViewModel(IView view)
        {
            this.View = view;
        }

        public ViewModel(BaseModel model)
        {
            this.ID = model.ID;
            this.Model = model;
        }

        private IView _view;
        /// <summary>
        /// 对应的View
        /// </summary>
        public IView View
        {
            get { return _view; }
            set
            {
                _view = value; this.OnPropertyChanged(); 
            } 
        }
         
        /// <summary>
        /// 对应的Model
        /// </summary>
        public BaseModel Model { get; private set; }
         
    }

   

    /// <summary>
    /// View对象接口
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// 对应的VM
        /// </summary>
        ViewModel ViewModel { get; } 
    }

    /// <summary>
    /// ListView对象接口
    /// </summary>
    public interface IListView:IView
    {
        /// <summary>
        /// 获取子项view
        /// </summary>
        /// <returns></returns>
        IView GetItemView(ViewModel vm);
        
    }

}
