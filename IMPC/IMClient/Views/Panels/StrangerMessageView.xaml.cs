using IMClient.ViewModels;
using IMClient.Views.ChildWindows;
using IMCustomControls;
using IMModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IMClient.Views.Panels
{
    /// <summary>
    /// StrangerMessageView.xaml 的交互逻辑
    /// </summary>
    public partial class StrangerMessageView : UserControl, IView
    {
        
        public StrangerMessageView()
        {
            InitializeComponent();

            this.DataContext = this.ViewModel = AppData.MainMV.ChatListVM.StrangerMessage;
            AppData.MainMV.ChatListVM.StrangerMessage.OnResetStrangerMessageListSort += StrangerMessage_OnResetStrangerMessageListSort;
            //AppData.MainMV.ChatListVM.StrangerMessage.OnDelegateToStrangerMessageView += StrangerMessage_OnDelegateToStrangerMessageView;
            this.Loaded += StrangerMessageView_Loaded;
            this.Unloaded += StrangerMessageView_Unloaded;
        }

        private void StrangerMessage_OnResetStrangerMessageListSort()
        {
            App.Current.Dispatcher.Invoke(() => 
            {
                ICollectionView cv = CollectionViewSource.GetDefaultView(this.list.Items);
                if (cv == null)
                {
                    return;
                }

                cv.SortDescriptions.Clear();
                cv.SortDescriptions.Add(new SortDescription("Model.LastMsg.SendTime", ListSortDirection.Descending));
            });            
        }

        private void StrangerMessageView_Unloaded(object sender, RoutedEventArgs e)
        {
            (AppData.MainMV.View as MainWindow).AppendWindowView?.Close();
        }

        //private void StrangerMessage_OnDelegateToStrangerMessageView()
        //{
        //    App.Current.Dispatcher.Invoke(() => 
        //    {
        //        this.SetExpandButtonHideOrDisplay();
        //    });            
        //}

        private static T FindSingleVisualChildren<T>(DependencyObject parentObj) where T : DependencyObject
        {
            T result = null;
            if (parentObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parentObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parentObj, i);
                    if (child != null && child is T)
                    {
                        result = child as T;
                        break;
                    }
                    result = FindSingleVisualChildren<T>(child);
                    if (result != null)
                        break;
                }
            }
            return result;
        }

        //private void SetExpandButtonHideOrDisplay()
        //{
        //    try
        //    {
        //        List<ItemsControl> ics = new List<ItemsControl>();
        //        List<ToggleButton> btns = new List<ToggleButton>();
        //        for (int i = 0; i < this.list.Items.Count; i++)
        //        {
        //            var item = this.list.ItemContainerGenerator.ContainerFromIndex(i);
        //            ItemsControl ic = FindSingleVisualChildren<ItemsControl>(item);
        //            ics.Add(ic);
        //        }
        //        foreach (ItemsControl child in ics)
        //        {
        //            List<SuperSelectableTextBlock> suPerLi = new List<SuperSelectableTextBlock>();
        //            List<ToggleButton> tgBLi = new List<ToggleButton>();
        //            for (int i = 0; i < child.Items.Count; i++)
        //            {
        //                var item = child.ItemContainerGenerator.ContainerFromIndex(i);
        //                SuperSelectableTextBlock t = FindSingleVisualChildren<SuperSelectableTextBlock>(item);
        //                suPerLi.Add(t);
        //                ToggleButton tgb = FindSingleVisualChildren<ToggleButton>(child);
        //                tgBLi.Add(tgb);
        //            }
        //            foreach (SuperSelectableTextBlock item in suPerLi)
        //            {
        //                foreach (ToggleButton item1 in tgBLi)
        //                {
        //                    if(Helper.TextBlockHelper.HasTextTrimmed(item))
        //                    {
        //                        if (item.DataContext == item1.DataContext)
        //                            item.Visibility = Visibility.Visible;   
        //                    }
        //                }
        //            }
        //            //foreach (SuperSelectableTextBlock item in suPerLi)
        //            //{
        //            //    if (Helper.TextBlockHelper.HasTextTrimmed(item))
        //            //    {
        //            //        var button = btns.FirstOrDefault(x => x.DataContext == child.DataContext);
        //            //        if (button != null)
        //            //        {
        //            //            button.Visibility = Visibility.Visible;
        //            //        }
        //            //        break;
        //            //    }
        //            //}
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        App.Logger.Error(ex.StackTrace);
        //    }
        //}

        private void StrangerMessageView_Loaded(object sender, RoutedEventArgs e)
        {
            this.StrangerMessage_OnResetStrangerMessageListSort();
            //this.SetExpandButtonHideOrDisplay();
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

        public ViewModel ViewModel { get; private set; }

        const double _appendWidth = 288;
        private void ptbtnAppend_Click(object sender, RoutedEventArgs e)
        {
            if (this.ptbtnAppend.IsChecked == true && this.ViewModel is ChatViewModel vm)
            {
                object content = null;
                vm.TargetVM = AppData.MainMV.GroupListVM.Items.ToList().FirstOrDefault(info => info.ID == vm.ID);
                content = new SetupFriendView() { DataContext = vm };
                this.ShowAppend(content, new Action(() => { this.ptbtnAppend.IsChecked = false; }));
            }
            else
            {
                this.ptbtnAppend.IsChecked = false;
                (AppData.MainMV.View as MainWindow).AppendWindowView?.Close();
            }
        }

        private void ShowAppend(object content, Action closed, double width = _appendWidth)
        {
            Point p = this.rectTemp.PointToScreen(new Point(0, 0));
            p.X /= Helper.PrimaryScreen.DpiXRate;
            p.Y /= Helper.PrimaryScreen.DpiYRate;

            bool isInner = false;
            double height = this.rectTemp.ActualHeight;
            if (Helper.PrimaryScreen.WorkingArea.Width / Helper.PrimaryScreen.DpiXRate - p.X < width)
            {
                p.X -= width;
                p.Y += 60;
                isInner = true;
                height -= 60;
            }

            Rect rect = new Rect(p.X, p.Y, width, height);

            //AppendWindow win = new AppendWindow(rect, isInner, content);
            (AppData.MainMV.View as MainWindow).AppendWindowView?.Close();
            (AppData.MainMV.View as MainWindow).AppendWindowView = new AppendWindow(rect, isInner, content);
            AppData.MainMV.IsOpenedAppendWindow = true;

            (AppData.MainMV.View as MainWindow).AppendWindowView.Closed += delegate
            {
                AppData.MainMV.IsOpenedAppendWindow = false;
                Task.Delay(1).ContinueWith(task =>
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        closed?.Invoke();
                    }));
                });
            };
            (AppData.MainMV.View as MainWindow).AppendWindowView.Show();
        }

        private void tBtnAppend_Click(object sender, RoutedEventArgs e)
        {
            List<ItemsControl> ics = new List<ItemsControl>();
            List<ToggleButton> btns = new List<ToggleButton>();
            for (int i = 0; i < this.list.Items.Count; i++)
            {
                var item = this.list.ItemContainerGenerator.ContainerFromIndex(i);
                ItemsControl ic = FindSingleVisualChildren<ItemsControl>(item);
                ics.Add(ic);
            }
            List<List<SuperSelectableTextBlock>> superTextBlocks = new List<List<SuperSelectableTextBlock>>();
            //List<List<SelectableTextBlock>> selectableTextBlocks = new List<List<SelectableTextBlock>>();
            List<List<ToggleButton>> btnss = new List<List<ToggleButton>>();
            foreach (ItemsControl child in ics)
            {
                List<SuperSelectableTextBlock> s = new List<SuperSelectableTextBlock>();
                List<ToggleButton> t = new List<ToggleButton>();
                //List<SelectableTextBlock> k = new List<SelectableTextBlock>();
                for (int i = 0; i < child.Items.Count; i++)
                {
                    var obj = child.ItemContainerGenerator.ContainerFromIndex(i);
                    s.Add(FindSingleVisualChildren<SuperSelectableTextBlock>(obj));
                    t.Add(FindSingleVisualChildren<ToggleButton>(obj));
                    //k.Add(FindSingleVisualChildren<SelectableTextBlock>(obj));
                }
                superTextBlocks.Add(s);
                btnss.Add(t);
                //selectableTextBlocks.Add(k);
            }
            ToggleButton btn = sender as ToggleButton;
            foreach (List<SuperSelectableTextBlock> child in superTextBlocks)
            {
                foreach (SuperSelectableTextBlock child1 in child)
                {
                    
                    if(child1.DataContext==btn.DataContext)
                    {
                        int j = child.IndexOf(child1);
                        int i = superTextBlocks.IndexOf(child);
                        SelectableTextBlock result = superTextBlocks[i][j];
                        if (btn.IsChecked == true)
                        {
                            child1.Height = double.NaN;
                            
                            //child1.Visibility = Visibility.Collapsed;
                            //result.Visibility = Visibility.Visible;
                            //result.TextTrimming = TextTrimming.None;
                            //result.Height = double.NaN;
                            //result.TextWrapping = TextWrapping.WrapWithOverflow;


                            //MessageModel msg = (this.DataContext as ChatViewModel).StrangerMessageList[i].Chat.Messages[j];
                            //Binding binding = new Binding();
                            //binding.Source = msg;
                            //binding.Path = new PropertyPath("Content");
                            //binding.Converter =new Converter.MessageToViewConverter();
                            //BindingOperations.SetBinding(result, SelectableTextBlock.ContentProperty, binding);
                            
                        }
                        else
                        {
                            child1.Height = 30;
                            //child1.Visibility = Visibility.Visible;
                            //result.Visibility = Visibility.Collapsed;
                            //result.Height = 30;
                        }
                    }
                }
            }
            
        }
    }
}
