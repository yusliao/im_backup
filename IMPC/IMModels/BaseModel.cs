using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IMModels
{
    /// <summary>
    /// 数据模型基础类
    /// </summary>
    public abstract class BaseModel: INotifyPropertyChanged
    {
        private int _id;
        /// <summary>
        /// 唯一标识ID
        /// </summary>
        public int ID
        {
            get { return _id; }
            set {  _id = value; this.OnPropertyChanged(); }
        }
         
        
        ///// <summary>
        ///// 属性变更通知.
        ///// 用法： this.OnPropertyChanged(p => this.PropertyName);
        ///// </summary>
        ///// <param name="propery"></param>
        //protected void OnPropertyChanged(Expression<Func<Object, Object>> propery)
        //{
        //    var body = propery.Body.ToString();
        //    string propertyName = body.Substring(body.LastIndexOf(".") + 1);
        //    this.OnPropertyChanged(propertyName);
        //}
         
        public event PropertyChangedEventHandler PropertyChanged;
        //暂时不公开，有需要再考量
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName=null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        } 
    }
}
