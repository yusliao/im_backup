using IM.Emoje.Controls;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

namespace IM.Emoje
{
    /// <summary>
    /// EmojeTabControl.xaml 的交互逻辑
    /// </summary>
    public partial class EmojeTabControl : UserControl
    {
        public event EventHandler Close;

        private EmojiEntity selectEmoji = new EmojiEntity();
        private ObservableCollection<EmojiEntity> _emojiList;

        /// <summary>
        /// 选中项
        /// </summary>
        public EmojiEntity SelectEmoji
        {
            get { return selectEmoji; }
            set { selectEmoji = value; }
        }
        /// <summary>
        /// emoji集合
        /// </summary>
        public ObservableCollection<EmojiEntity> EmojiList
        {
            get { return _emojiList; }
            set { _emojiList = value; }
        }

        public EmojeTabControl()
        {
            InitializeComponent();

            AnalysisXML anlyxml = new AnalysisXML();
            anlyxml.LoadDefaultExpression();
            EmojiList = new ObservableCollection<EmojiEntity>(anlyxml.EmojiList);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb?.SelectedItem != null)
            {
                SelectEmoji = (EmojiEntity)lb.SelectedItem;
                Close?.Invoke(this, null);
            }
        }

        private void img_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is AnimatedGIF)
            {
                AnimatedGIF img = sender as AnimatedGIF;
                img.StartAnimation();
            }
            else
            {
                GifImage img = sender as GifImage;
                img.IsActive = true;//.gifAnimation?.StartAnimation();
            }
        }

        private void img_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is AnimatedGIF)
            {
                AnimatedGIF img = sender as AnimatedGIF;
                img.StopAnimation();
            }
            else
            {
                GifImage img = sender as GifImage;
                img.IsActive = false;//.gifAnimation?.StopAnimation();
            }
        }

        private void img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(sender is GifImage img)
            {
                SelectEmoji = img.DataContext as EmojiEntity;
                Close?.Invoke(this, null);
            }
        }
    }
}
