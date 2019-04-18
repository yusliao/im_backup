using IMClient.ViewModels;
using IMClient.Views.ChildWindows;
using IMModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IMClient.Helper;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// GroupNoticeView.xaml 的交互逻辑
    /// </summary>
    public partial class GroupNoticeView : UserControl
    {
        public GroupNoticeView()
        {
            InitializeComponent();
            this.Loaded += GroupNoticeView_Loaded;
        }

        private void GroupNoticeView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDatas();
        }

        private async void LoadDatas()
        {
            if (this.DataContext is ChatViewModel cvm && cvm.TargetVM is GroupViewModel)
            {
                GroupViewModel gvm = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(x => x.ID == (cvm.TargetVM as GroupViewModel).ID);
                if (gvm.AllNotice.Count > 0)
                {
                    gvm.AllNotice.Clear();
                }
                GroupModel gm = gvm.Model as GroupModel;
                App.Current.Dispatcher.Invoke(() => {
                    this.aniLoading?.Begin();
                });
                await SDKClient.SDKClient.Instance.GetNoticeList_DESC(gm.ID, 0, 0, null, HandleCompleteCallBack: (r) =>
                {
                    if (r.success)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            this.aniLoading.Stop();
                            this.sv.Visibility = Visibility.Visible;
                            this.grid_LoadFail.Visibility = Visibility.Collapsed;
                        });
                        var pacageDatas = r.datas;
                        List<SDKClient.DTO.NoticeEntity> dataList = new List<SDKClient.DTO.NoticeEntity>();
                        if (pacageDatas.Any(x => x.type == 1))
                        {
                            dataList = pacageDatas.Where(x => x.type != 1).ToList();
                        }
                        else
                        {
                            dataList = pacageDatas.ToList();
                        }

                        if (dataList.Count > 0)
                        {
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                this.aniLoading.Stop();
                                foreach (SDKClient.DTO.NoticeEntity item in dataList)
                                {
                                    GroupNotice gn = new GroupNotice();
                                    gn.GroupId = item.groupId;
                                    gn.NoticeTitle = item.title;
                                    gn.NoticeContent = item.content;
                                    gn.NoticeReleTime = item.publishTime ?? DateTime.Now;
                                    gn.NoticeType = item.type;
                                    gn.NoticeId = item.noticeId;
                                    gvm.AllNotice.Add(gn);
                                }
                            }));
                        }
                        if (pacageDatas.Any(x => x.type == 1))
                        {
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                this.aniLoading.Stop();
                                var item = pacageDatas.FirstOrDefault(x => x.type == 1);
                                GroupNotice gn = new GroupNotice();
                                gn.GroupId = item.groupId;
                                gn.NoticeTitle = item.title;
                                gn.NoticeContent = item.content;
                                gn.NoticeReleTime = item.publishTime ?? DateTime.Now;
                                gn.NoticeType = item.type;
                                gn.NoticeId = item.noticeId;
                                gvm.AllNotice.Insert(0, gn); 
                            })); 
                            gvm.IsShowEmptyNotice = false;
                            gvm.IsShowEmptyNoticeOrdinary = false;
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                this.aniLoading.Stop();
                            });
                            int count = pacageDatas.Count;
                            if (gvm.IsCreator)
                            {
                                if (count != 0)//没有入群须知，只有普通群公告
                                {
                                    gvm.IsShowEmptyNotice = true;
                                    gvm.IsShowEmptyNoticeOrdinary = false;
                                }
                                else//没有入群须知，也没有普通群公告
                                {
                                    gvm.IsShowEmptyNotice = true;
                                    gvm.IsShowEmptyNoticeOrdinary = false;
                                }
                            }
                            else
                            {
                                if (count != 0)//没有入群须知，只有普通群公告
                                {
                                    gvm.IsShowEmptyNoticeOrdinary = false;
                                    gvm.IsShowEmptyNotice = false;
                                }
                                else//没有入群须知，也没有普通群公告
                                {
                                    gvm.IsShowEmptyNoticeOrdinary = true;
                                    gvm.IsShowEmptyNotice = false;
                                }
                            }
                        }  
                    }
                    else
                    {
                        //提示请求失败
                        App.Current.Dispatcher.Invoke(() => {
                            this.aniLoading.Stop();
                            this.sv.Visibility = Visibility.Collapsed;
                            this.grid_LoadFail.Visibility = Visibility.Visible;
                        });
                        if (gvm.IsCreator)
                        {
                            gvm.IsShowEmptyNotice = true;
                            gvm.IsShowEmptyNoticeOrdinary = false;
                        }
                        else
                        {
                            gvm.IsShowEmptyNotice = false;
                            gvm.IsShowEmptyNoticeOrdinary = true;
                        }
                        
                    }
                }).ConfigureAwait(false);
            }
        }

        public bool IsTextBlockTrimmed(TextBlock textBlock)
        {
            if (textBlock.DesiredSize.Height > 69)
                return true;
            return false;
        }

        /// <summary>
        /// 添加普通群公告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bd_AddNotice_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is ChatViewModel cvm && cvm.TargetVM is GroupViewModel)
            {
                GroupModel gm = (cvm.TargetVM as GroupViewModel).Model as GroupModel;
                GroupNotice gn = new GroupNotice()
                {
                    GroupId = gm.ID,
                    GroupName = gm.DisplayName,
                    NoticeReleTime = DateTime.Now,
                    IsCanOperate = true,
                    NoticeType = 0
                };
                GroupNoticeWindow gnw = new GroupNoticeWindow(gn);
                gnw.Owner = App.Current.MainWindow;
                gnw.ShowDialog();
            }
        }

        /// <summary>
        /// 添加入群须知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hyper_Link_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.DataContext is ChatViewModel cvm && cvm.TargetVM is GroupViewModel)
                {
                    GroupViewModel gvm = (this.DataContext as ChatViewModel).TargetVM as GroupViewModel;
                    GroupViewModel gvm1 = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(x => x.ID == gvm.ID);
                    GroupModel groupModel = gvm1.Model as GroupModel;
                    GroupNotice gnTice = gvm1.AllNotice.FirstOrDefault(x => x.NoticeTitle == "入群须知");
                    if (gnTice != null)
                    {
                        AppData.MainMV.TipMessage = "已添加入群须知";
                        return;
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            GroupModel gm = (cvm.TargetVM as GroupViewModel).Model as GroupModel;
                            GroupNotice gn = new GroupNotice()
                            {
                                NoticeTitle = "入群须知",
                                GroupId = gm.ID,
                                GroupName = gm.DisplayName,
                                NoticeReleTime = DateTime.Now,
                                IsCanOperate = true,
                                NoticeType = 1
                            };
                            if (!gvm.IsCreator)
                            {
                                gn.IsCanOperate = false;
                            }
                            GroupNoticeWindow gnw = new GroupNoticeWindow(gn, "添加入群须知");
                            gnw.txb_Title.IsEnabled = false;
                            gnw.Owner = App.Current.MainWindow;
                            gnw.ShowDialog();
                        }));
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.ShowDialogBox(ex.Message);
            }

        }

        GroupNotice waitRemoveNotice;

        /// <summary>
        /// 删除公告
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>        
        private void hyperLink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            GroupNotice gn = waitRemoveNotice = link.DataContext as GroupNotice;
            Border bder = ((link.Parent as TextBlock).Parent as Grid).Parent as Border;
            if (gn != null)
            {
                if (IMClient.Views.MessageBox.ShowDialogBox("删除后不可恢复，确定删除？"))
                    SDKClient.SDKClient.Instance.DeleteNotice(gn.NoticeId, gn.GroupId, gn.GroupName, gn.NoticeTitle, (result) =>
                    {
                        if (result.Item1)
                        {
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                BeginAnimation(bder);
                            }));
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                AppData.MainMV.TipMessage = "删除失败！";
                            }));
                        }
                    }, gn.NoticeType == 0 ? SDKClient.SDKProperty.NoticeType.Common : SDKClient.SDKProperty.NoticeType.Common);
                else
                    return;
            }
        }

        public void BeginAnimation(Border border)
        {
            DoubleAnimation DAnimation = new DoubleAnimation();
            DAnimation.From = 0;//起点
            DAnimation.To = border.ActualWidth;//终点
            DAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));//时间

            var transform = border.RenderTransform as TranslateTransform;
            if (transform == null)
                border.RenderTransform = transform = new TranslateTransform();

            DependencyProperty[] propertyChain = new DependencyProperty[]
            {
                Border.RenderTransformProperty,
                TranslateTransform.XProperty
            };

            Storyboard.SetTarget(DAnimation, border);
            Storyboard.SetTargetProperty(DAnimation, new PropertyPath("(0).(1)", propertyChain));
            Storyboard story = new Storyboard();
            story.Completed += Story_Completed;
            story.Children.Add(DAnimation);
            story.Begin();
        }

        private void Story_Completed(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                AppData.MainMV.TipMessage = "删除成功！";
                GroupViewModel gvm = (this.DataContext as ChatViewModel).TargetVM as GroupViewModel;
                GroupViewModel gvm1 = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(x => x.ID == gvm.ID);
                gvm1.AllNotice.Remove(this.waitRemoveNotice);
                if (gvm1.AllNotice.Any(x => x.NoticeType == 1))
                {
                    if (gvm1.IsCreator)
                    {
                        gvm1.IsShowEmptyNotice = false;
                        gvm1.IsShowEmptyNoticeOrdinary = false;
                    }
                }
                else//没有入群须知
                {
                    if (gvm1.AllNotice.Count == 0)
                    {
                        if (gvm1.IsCreator)
                        {
                            gvm1.IsShowEmptyNotice = true;
                            gvm1.IsShowEmptyNoticeOrdinary = false;
                        }
                        else
                        {
                            gvm1.IsShowEmptyNotice = false;
                            gvm1.IsShowEmptyNoticeOrdinary = true;
                        }
                    }
                    else
                    {
                        if (gvm1.IsCreator)
                        {
                            gvm1.IsShowEmptyNotice = true;
                            gvm1.IsShowEmptyNoticeOrdinary = false;
                        }
                        else
                        {
                            gvm1.IsShowEmptyNotice = false;
                            gvm1.IsShowEmptyNoticeOrdinary = true;
                        }
                    }
                }
            }));
        }

        public T GetChildObject<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;
            T grandChild = null;
            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);
                if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)child;
                }
                else
                {
                    grandChild = GetChildObject<T>(child, name);
                    if (grandChild != null)
                        return grandChild;
                }
            }
            return null;
        }

        public List<T> GetChildObjects<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);

                if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
                {
                    childList.Add((T)child);
                }
                childList.AddRange(GetChildObjects<T>(child, name));
            }
            return childList;
        }

        private void tBtnExpand_Click(object sender, RoutedEventArgs e)
        {
            List<SuperTextBlock> superTextBlocks = GetChildObjects<SuperTextBlock>(this.ic_AllNotice, "txb_NoticeContent1");
            List<ToggleButton> btns = GetChildObjects<ToggleButton>(this.ic_AllNotice, "tBtnExpand");
            ToggleButton btn = sender as ToggleButton;
            int i = btns.IndexOf(btn);
            SuperTextBlock superTextBlock = superTextBlocks[i];
            if (btn.IsChecked == true)
            {
                superTextBlock.Height = double.NaN;
            }
            else
            {
                superTextBlock.Height = 60;
            }
        }

        private void tBtnExpand_MouseEnter(object sender, MouseEventArgs e)
        {
            ToggleButton tgbn = sender as ToggleButton;
            ToolTip tt = new ToolTip();
            if (tgbn.IsChecked == true)
                tt.Content = "收起";
            else
                tt.Content = "展开";
            tgbn.ToolTip = tt;
        }

        private void hp_ReLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadDatas();
        }
    }
}
