using IMClient.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// HelpCenterView.xaml 的交互逻辑
    /// </summary>
    public partial class HelpCenterView : UserControl,IView
    {
        public HelpCenterView()
        {
            InitializeComponent();
            this.DataContext = this.ViewModel = new HelpCenterViewModel(new HelpCenterModel());
            this.Loaded += HelpCenterView_Loaded;
            
        }

        private void HelpCenterView_Loaded(object sender, RoutedEventArgs e)
        {
            //this.m_webFrame.Source = new Uri("http://192.168.4.24/Help/Index", UriKind.RelativeOrAbsolute);
            //this.m_webFrame.LoadCompleted += M_webFrame_LoadCompleted;



            //string text = @"<html><title>我的第一个网页</title><head></head><body><p>hahhahh</p></body></html>";
            //string xaml = HTMLConverter.HtmlToXamlConverter.ConvertHtmlToXaml(text, true);
            //rssBlogText.Document = XamlReader.Load(new XmlTextReader(new StringReader(xaml))) as FlowDocument;


            //this.fra_Content.Navigate(new Uri("https://www.baidu.com/", UriKind.RelativeOrAbsolute));
            //this.fra_Content.Source = new Uri("https://www.baidu.com/", UriKind.RelativeOrAbsolute);
        }

        private void M_webFrame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Frame frame = sender as Frame;
            WebBrowser webBroser = frame.Content as WebBrowser;
            
        }

       

        public ViewModel ViewModel { get; private set; }

      
    }
}
