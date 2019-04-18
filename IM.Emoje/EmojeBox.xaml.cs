using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// EmojeBox.xaml 的交互逻辑
    /// </summary>
    public partial class EmojeBox : UserControl
    {
        public readonly static List<EmojeItem> EmojeKeys;

        private static List<EmojeGroup> Groups;
        static EmojeBox()
        {
            EmojeKeys = new List<EmojeItem>();
            if (EmojeKeys.Count == 0)
            {
                Groups = new List<EmojeGroup>();
                Groups.Add(LoadDefaultEmojes(0, "IM.Emoje;component/EmojeDictionary.xaml"));
            }
        }

        public EmojeBox()
        {
            InitializeComponent();

            this.tabFrame.Items.Clear();

            //List<EmojeGroup> Groups = new List<EmojeGroup>();
            //Groups.Add(LoadDefaultEmojes(0, "IM.Emoje;component/EmojeDictionary.xaml"));
            this.tabFrame.ItemsSource = Groups;
            this.tabFrame.SelectedIndex = 0;
        }

        private static EmojeGroup LoadDefaultEmojes(int index, string dictionary)
        {
            ResourceDictionary resDictionary = new ResourceDictionary();
            resDictionary.Source = new Uri(dictionary, UriKind.Relative);

            List<EmojeItem> items = new List<EmojeItem>();
           
            foreach (System.Collections.DictionaryEntry item in resDictionary)
            {
                string[] keys = $"{item.Key}".Split('_');

                int.TryParse(keys[0], out int id);

                string key = $"[{keys[1]}]";

                EmojeItem emoje = new EmojeItem()
                {
                    Index = id,
                    Name = key,
                    Path = $"{item.Value}",
                    ImageSource = item.Value as BitmapImage
                };

                items.Add(emoje);
                EmojeKeys.Add(emoje);
            }

            EmojeGroup group = new EmojeGroup()
            {
                Index = index,
                Name = "默认表情",
                Items = items.OrderBy(info => info.Index),
            };

            var normal = items.FirstOrDefault(info => info.Name.Contains("酷"));
            if (normal == null)
            {
                normal = items.FirstOrDefault();
            }
            group.Path = normal.Path;

            return group;
        }

        private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image img && img.DataContext is EmojeItem item)
            {
                this.SelectEmoje = item;
            }
        }

        private EmojeItem _selectEmoje;
        public EmojeItem SelectEmoje
        {
            get { return _selectEmoje; }
            set
            {
                _selectEmoje = value;
                if (_selectEmoje != null)
                {
                    Selected?.Invoke(_selectEmoje);
                }
            }
        }

        public Action<EmojeItem> Selected;
    }


}
