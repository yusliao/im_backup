using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IM.Emoje
{
    public class EmojeItem : INotifyPropertyChanged
    {
        private int _index;
        /// <summary>
        /// 序号
        /// </summary>
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        private string _name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        private string _path;

        /// <summary>
        /// 路径
        /// </summary>
        public string Path
        {
            get { return _path; }
            set { _path = value; OnPropertyChanged(); }
        }
        
        public BitmapImage ImageSource { get;internal set; }


        public event PropertyChangedEventHandler PropertyChanged;
        //暂时不公开，有需要再考量
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class EmojeGroup: EmojeItem
    {
        private IEnumerable<EmojeItem> _items;

        public IEnumerable<EmojeItem> Items
        {
            get { return _items; }
            set { _items = value; OnPropertyChanged(); }
        }

    }
}
