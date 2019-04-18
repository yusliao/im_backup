
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IM.Emoje
{
    public class AnalysisXML
    {
        private static List<EmojiEntity> emojiList = new List<EmojiEntity>();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// emoji集合
        /// </summary>
        public List<EmojiEntity> EmojiList
        {
            get { return emojiList; }
            set { emojiList = value; }
        }

        public void LoadDefaultExpression()
        {
            if (EmojiList.Count == 0)
            {
                var res = Properties.Resources._1_调皮;//千万不要注释或删除这句话（为了预加载Resources）

                EmojiEntity entity = new EmojiEntity();

                foreach (System.Collections.DictionaryEntry item in Properties.Resources.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, false, false))
                {
                    if (item.Value.GetType() == typeof(System.Drawing.Bitmap))
                    {
                        string key = item.Key.ToString().TrimStart('_');
                        int.TryParse(key.Split('_')[0], out int index);

                        string path = $@"pack://application:,,,/IM.Emoje;component/Image/{key}.png";

                        //System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)item.Value;
                        EmojiEntity emoji = new EmojiEntity()
                        {
                            Index = index,
                            Key = $"/{key.Split('_')[1]}",
                            KeyImg = (System.Drawing.Bitmap)item.Value,
                            ImagePath = path,
                            ImgWidth = 30,// bitmap.Width,
                            ImgHeight = 30,// bitmap.Height,
                        };
                        entity.Emojis.Add(emoji);
                    }
                }
                entity.Key = "默认表情";
                entity.Emojis = new ObservableCollection<EmojiEntity>(entity.Emojis.OrderBy(x => x.Index).ToList());

                var normal = entity.Emojis.FirstOrDefault(info => info.Key.Contains("酷"));
                if (normal == null)
                {
                    normal = entity.Emojis.FirstOrDefault();
                }
                entity.KeyImg = normal.KeyImg;
                entity.ImagePath = normal.ImagePath;

                EmojiList.Add(entity);

                LoadEmojis();

                if (ContantClass.EmojiCode.Count == 0)
                {
                    foreach (var item in EmojiList)
                    {
                        foreach (var emoji in item.Emojis)
                        {
                            ContantClass.EmojiCode.Add(emoji);
                        }
                    }
                }
            }
        }

        public void LoadEmojis()
        {
            string emojiPath = Path.Combine(SDKClient.SDKProperty.rootPath, "Emotion");

            if (!Directory.Exists(emojiPath))
            {
                //logger.Error("程序缺少Emotion文件夹");
                return;
            }

            DirectoryInfo dirInfo = new DirectoryInfo(emojiPath);
            foreach (var item in dirInfo.GetDirectories())
            {
                EmojiEntity entity = new EmojiEntity();
                foreach (var file in item.GetFiles())
                {
                    string filePath = file.FullName;
                    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(filePath);

                    EmojiEntity emoji = new EmojiEntity()
                    {
                        Key = $"/{file.Name.Split('.')[0]}",
                        KeyImg = bmp,
                        ImagePath = filePath,
                        ImgWidth = bmp.Width / 4,
                        ImgHeight = bmp.Height / 4,
                    };

                    if (!ContantClass.NonDefaultExpressions.Contains(emoji.Key))
                    {
                        ContantClass.NonDefaultExpressions.Add(emoji.Key);
                    }

                    entity.Emojis.Add(emoji);
                }

                if (item.FullName.Split('\\').Length > 0)
                {
                    entity.Key = item.FullName.Split('\\')[item.FullName.Split('\\').Length - 1];
                }

                string keyImgPath = Path.Combine(emojiPath, entity.Key) + ".png";
                if (File.Exists(keyImgPath))
                {
                    entity.KeyImg = new System.Drawing.Bitmap(keyImgPath);
                    entity.ImagePath = keyImgPath;
                }
                else
                {
                    logger.Error(string.Format("程序缺少{0}文件", Path.GetFileName(keyImgPath)));
                }

                EmojiList.Add(entity);
            }
        }
    }
}
