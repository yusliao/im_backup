using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace IM.Emoje
{
    public class EmojiEntity
    {
        private int _index;
        private string _key;
        private System.Drawing.Bitmap _keyImg;
        private ObservableCollection<EmojiEntity> _emojis = new ObservableCollection<EmojiEntity>();
        private int _imgWidth;
        private int _imgHeight;

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }
        /// <summary>
        /// 名称
        /// </summary>
        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
            }
        }
        /// <summary>
        /// 图像
        /// </summary>
        public System.Drawing.Bitmap KeyImg
        {
            get
            {
                return _keyImg;
            }
            set
            {
                _keyImg = value;
            }
        }

        /// <summary>
        /// 图像
        /// </summary>
        public string ImagePath { get; set; }

        private BitmapImage _imageSource;
        public BitmapImage ImageSorce
        {
            get
            {
                if (_imageSource == null && !string.IsNullOrEmpty(this.ImagePath))
                {
                    _imageSource = new BitmapImage(new Uri(this.ImagePath, UriKind.RelativeOrAbsolute));
                }
                return _imageSource;
            }
        }

        public ObservableCollection<EmojiEntity> Emojis
        {
            get { return _emojis; }
            set { _emojis = value; }
        }
        /// <summary>
        /// 图片宽度
        /// </summary>
        public int ImgWidth
        {
            get { return _imgWidth; }
            set { _imgWidth = value; }
        }
        /// <summary>
        /// 图片高度
        /// </summary>
        public int ImgHeight
        {
            get { return _imgHeight; }
            set { _imgHeight = value; }
        }
    }
}
