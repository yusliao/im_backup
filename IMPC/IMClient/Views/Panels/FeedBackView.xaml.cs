using IMClient.ViewModels;
using System;
using System.Collections.Generic;
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
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// FeedBackView.xaml 的交互逻辑
    /// </summary>
    public partial class FeedBackView : UserControl,IView
    {
        public FeedBackView()
        {
            InitializeComponent();
            this.Loaded += FeedBackView_Loaded;
            if (AppData.feedBackVmDic.ContainsKey(AppData.Current.LoginUser.User.ID))
                this.DataContext = AppData.feedBackVmDic[AppData.Current.LoginUser.User.ID];
            else
                this.DataContext = this.ViewModel = new FeedBackViewModel(new FeedBackModel());
        }

        private void FeedBackView_Loaded(object sender, RoutedEventArgs e)
        {
            CreateSubmitInfo();
        }

        public ViewModel ViewModel { get; private set; }

        private void txb_Content_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            int length = tb.Text.Length;
            this.txb_Input.Text = length.ToString();
            //tb.Text = tb.Text.Replace(" ", "");//去掉所有的空格
            //tb.SelectionStart = tb.Text.Length;//尝试将光标移到结尾处
            if (!string.IsNullOrEmpty(tb.Text))
                this.btnSure.IsEnabled = true;
            else
                this.btnSure.IsEnabled = false;
        }

        private bool CheckDataLegal(FeedBackViewModel fvm)
        {
            if (!string.IsNullOrEmpty(fvm.FeedBackContent))
            {
                int contentLegth = fvm.FeedBackContent.Replace(" ", "").Length;
                if (contentLegth < 5)
                {
                    App.Current.Dispatcher.Invoke(new Action(()=> {
                        AppData.MainMV.TipMessage = "内容过短！";
                    }));
                    return false;
                }
                return true;
            }
            else
            {
                this.btnSure.IsEnabled = false;
                return false;
            }
        }

        private async void btnSure_Click(object sender, RoutedEventArgs e)
        {
            FeedBackViewModel fvm = this.DataContext as FeedBackViewModel;
            if(AppData.CanInternetAction())
            {
                if(CheckDataLegal(fvm))
                {
                    bool b = await SDKClient.SDKClient.Instance.AddFeedBack(fvm.FeedBackContent);
                    if(IsFrequentSubmission())
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            AppData.MainMV.TipMessage = "请不要频繁提交！";
                            if (!AppData.feedBackVmDic.ContainsKey(AppData.Current.LoginUser.User.ID))
                                AppData.feedBackVmDic.Add(AppData.Current.LoginUser.User.ID, fvm);
                        }));
                    }
                    else
                    {
                        if (b)
                        {
                            App.Current.Dispatcher.Invoke(new Action(() => {
                                AppData.MainMV.TipMessage = "提交成功，谢谢你的反馈！";
                                SubmitSuccess();
                                if (AppData.feedBackVmDic.ContainsKey(AppData.Current.LoginUser.User.ID))
                                    AppData.feedBackVmDic.Remove(AppData.Current.LoginUser.User.ID);
                                fvm.FeedBackContent = string.Empty;
                            }));
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                if (!AppData.feedBackVmDic.ContainsKey(AppData.Current.LoginUser.User.ID))
                                    AppData.feedBackVmDic.Add(AppData.Current.LoginUser.User.ID, fvm);
                            }));
                        }
                    }    
                }
            }
            else
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    AppData.MainMV.TipMessage = "网络异常，请检查设置！";
                }));
            }
        }
        /// <summary>
        /// 创建记录反馈时间的xml文件
        /// </summary>
        private void CreateSubmitInfo()
        {
            string filepath = System.IO.Path.Combine(Environment.CurrentDirectory, "Tools/Dataconfig.xml");
            if(!File.Exists(filepath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement xmlRoot = xmlDoc.CreateElement("root");
                XmlElement xeRow = xmlDoc.CreateElement("", string.Format("row{0}", AppData.Current.LoginUser.ID.ToString()), "");
                xmlRoot.AppendChild(xeRow);
                xmlDoc.AppendChild(xmlRoot);
                xmlDoc.Save(filepath);
            }
            else
            {
                XDocument xdoc = XDocument.Load(filepath);
                if (xdoc.Root.Elements().Count(x => x.Name.LocalName.ToLower().Equals("row" + AppData.Current.LoginUser.ID.ToString())) > 0)
                {
                    return;
                }
                else
                {
                    XElement xRoot = xdoc.Root;
                    XElement xRow = new XElement(string.Format("row{0}", AppData.Current.LoginUser.ID.ToString()), "");
                    xRoot.Add(xRow);
                    xdoc.Save(filepath);
                }
            }
           
        }

        /// <summary>
        /// 是否频繁提交
        /// </summary>
        /// <returns></returns>
        private bool IsFrequentSubmission()
        {
            string filepath = System.IO.Path.Combine(Environment.CurrentDirectory, "Tools/Dataconfig.xml");
            XDocument xdoc = XDocument.Load(filepath);
            XElement xroot = xdoc.Root.Elements().FirstOrDefault(x => x.Name.LocalName.ToLower().Equals("row"+AppData.Current.LoginUser.ID.ToString()));
            if(xroot.Elements().Count()==0)
            {
                xroot.Add(new XElement("item", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                xdoc.Save(filepath);
                return false;
            }
            else
            {
                XElement xeLast = xroot.Elements().ToList().Last();
                TimeSpan dtSpan =DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) - DateTime.Parse(xeLast.Value);
                if (dtSpan.TotalSeconds <= 300)
                {
                    return true;
                }
            }
            return false ;
        }

        /// <summary>
        /// 提交成功后，写一次时间
        /// </summary>
        private void SubmitSuccess()
        {
            string filepath = System.IO.Path.Combine(Environment.CurrentDirectory, "Tools/Dataconfig.xml");
            XDocument xdoc = XDocument.Load(filepath);
            XElement xroot = xdoc.Root.Elements().FirstOrDefault(x => x.Name.LocalName.ToLower().Equals("row" + AppData.Current.LoginUser.ID.ToString()));
            XElement xRow = new XElement("item", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            if(xroot.Elements().Where(x=>x.Value.Equals(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))).Count()==0)
                xroot.Add(xRow);
            xdoc.Save(filepath);
        }
    }
}
