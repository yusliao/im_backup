using IMModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using System.IO;
using System.Windows.Documents;
using IMClient.Views.ChildWindows;

namespace IMClient.ViewModels
{
    public class HelpCenterViewModel : ViewModel
    {
        public HelpCenterViewModel(HelpCenterModel model) : base(model)
        {
            
        }
        #region 属性
        private string _quesclassFication;
        /// <summary>
        /// 问题分类
        /// </summary>
        public string QuesClassFication
        {
            get { return _quesclassFication; }
            set { _quesclassFication = value; this.OnPropertyChanged(); }
        }

        private string _quesTitle;
        /// <summary>
        /// 问题标题
        /// </summary>
        public string QuesTitle
        {
            get { return _quesTitle; }
            set { _quesTitle = value; this.OnPropertyChanged(); }
        }

        private string _quesContent= "http://192.168.4.24/Help/Index";
        /// <summary>
        /// 问题内容
        /// </summary>
        public string QuesContent
        {
            get { return _quesContent; }
            set { _quesContent = value; this.OnPropertyChanged(); }
        }
        #endregion

        #region 命令绑定
        private VMCommand _navigateToHTML;
        public VMCommand NavigateToHTML
        {
            get
            {
                if (_navigateToHTML == null)
                    _navigateToHTML = new VMCommand(NavigateToHTMLContent);
                return _navigateToHTML;
            }
        }
        #endregion

        #region 命令处理方法
        /// <summary>
        /// 导航到HTML页面
        /// </summary>
        /// <param name="para"></param>
        private void NavigateToHTMLContent(object para)
        {
            Dictionary<string, object> paraDic = para as Dictionary<string, object>;
            if (paraDic == null)
                return;
            string url = (string)paraDic["context1"];
            //Frame fdr = paraDic["context2"] as Frame;
            //Grid grid = paraDic["context3"] as Grid;
            //Border bd = paraDic["context4"] as Border;
            if (!string.IsNullOrEmpty(url))
            {
                //grid.Visibility = System.Windows.Visibility.Visible;
                //bd.Visibility = System.Windows.Visibility.Collapsed;
                //string str = new Util.Webs.Clients.WebClient().Get("https://docs.microsoft.com/zh-cn/dotnet/framework/wpf/graphics-multimedia/hit-testing-in-the-visual-layer").Result();
                //string text = @"<html><title>我的第一个网页</title><head>
                //</head><body><p>数据就在SVG的结点下面，我们把数据复制粘贴出来，这些数据上，我们能发现，是分离开的，举个例子：1号线上有很多个站点，例如苹果园、古城等，但是在数据上，你看不出1号线和苹果园、古城的关系所在，也就是说，展示给我们的数据是相对独立的。数据上，提供了Path的路径，Textblock和Ellipse的坐标信息。那我们就用这些信息，简单的画一下北京地铁图。</p></body></html>";
                //string xaml = HTMLConverter.HtmlToXamlConverter.ConvertHtmlToXaml(str, true);
                //fdr.Document = XamlReader.Load(new XmlTextReader(new StringReader(xaml))) as FlowDocument;
                System.Diagnostics.Process.Start(url);
            }
        }
        #endregion
       
    }

    public class HelpCenterModel: BaseModel
    { }
}
