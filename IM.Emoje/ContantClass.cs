using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IM.Emoje
{
    public class ContantClass
    {
        private static ObservableCollection<EmojiEntity> _emojiCode = new ObservableCollection<EmojiEntity>();//emoji编码
        /// <summary>
        /// emoji编码
        /// </summary>
        public static ObservableCollection<EmojiEntity> EmojiCode
        {
            get
            {
                return _emojiCode;
            }
            set
            {
                _emojiCode = value;
            }
        }

        /// <summary>
        /// 非默认表情集合（为了单独发送到服务端）
        /// </summary>
        public static List<string> NonDefaultExpressions = new List<string>();
    }
}
