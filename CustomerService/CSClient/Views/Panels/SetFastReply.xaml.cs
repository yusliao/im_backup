using CSClient.ViewModels;
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

namespace CSClient.Views.Panels
{
    /// <summary>
    /// SetFastReply.xaml 的交互逻辑
    /// </summary>
    public partial class SetFastReply : UserControl, IView
    {
        /*....................../´¯/) 
        ......................,/¯../ 
        ...................../..../ 
        .............../´¯/'...'/´¯¯`·¸ 
        ............/'/.../..../......./¨¯\ 
        ..........('(...´...´.... ¯~/'...') 
        ...........\.................'...../ 
        ............''...\.......... _.·´ 
        ..............\..............( 
        ................\.............\*/
        public SetFastReply(ViewModel vm)
        {
            InitializeComponent();

            this.DataContext = this.ViewModel = vm;
        }

        public ViewModel ViewModel { get; private set; }

        public TaskScheduler UItaskScheduler => throw new NotImplementedException();

        private void gridAdd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.AddEmptyContentItem();
        }

        private void AddEmptyContentItem()
        {
            if (this.DataContext is FastReplyViewModel vm)
            {
                FastReplyContentModel contentModel = new FastReplyContentModel();
                vm.SelectedType.TypeItems.Add(contentModel);
            }
        }

        private void gridRemove_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is FastReplyViewModel vm)
            {
                int editType = 3;
                int categoryType = vm.IsCommonReply ? 1 : 2;

                FastReplyContentModel contentModel = (sender as Grid).DataContext as FastReplyContentModel;
                vm.SelectedType.TypeItems.Remove(contentModel);
                if (vm.SelectedType.TypeItems.Count == 0)
                {
                    this.AddEmptyContentItem();
                }

                if (contentModel.ContentId <= 0)
                {
                    return;
                }

                var result = SDKClient.SDKClient.Instance.PostQuickReplyContentedit(editType, contentModel.ContentId, "", vm.SelectedType.TypeId);
                if (!result.success)
                {
                    AppData.MainMV.TipMessage = "移除快捷回复内容失败";
                }
                else
                {
                    AppData.MainMV.TipMessage = "移除快捷回复内容成功";
                }
            }
        }

        private void btnAddType_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.txtAddTypeName.Text))
            {
                AppData.MainMV.TipMessage = "类型名称不能为空";
                return;
            }

            if (this.DataContext is FastReplyViewModel vm)
            {
                string typeName = this.txtAddTypeName.Text.Trim();

                if (vm.TypeItems.Any(x => x.TypeName == typeName))
                {
                    AppData.MainMV.TipMessage = "类型名称重复，请重新输入";
                    return;
                }
                
                int editType = 1;
                int categoryType = vm.IsCommonReply ? 1 : 2;
                var result = SDKClient.SDKClient.Instance.PostQuickReplyCategoryedit(editType, 0, typeName, categoryType);
                if (result.success)
                {
                    FastReplyTypeModel typeModel = new FastReplyTypeModel()
                    {
                        TypeId = result.id,
                        TypeName = typeName,
                    };
                    FastReplyContentModel contentModel = new FastReplyContentModel();
                    typeModel.TypeItems.Add(contentModel);

                    vm.TypeItems.Add(typeModel);

                    AppData.MainMV.TipMessage = "新增回复类型成功";
                }
                else
                {
                    AppData.MainMV.TipMessage = "新增回复类型失败";
                }
            }
        }

        private void btnEditType_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.txtEditTypeName.Text))
            {
                AppData.MainMV.TipMessage = "类型名称不能为空";
                return;
            }

            if (this.DataContext is FastReplyViewModel vm)
            {
                string typeName = this.txtEditTypeName.Text.Trim();
                int cateId = vm.SelectedType.TypeId;
                if (vm.SelectedType.TypeName.Equals(typeName))
                {
                    return;
                }

                if (vm.TypeItems.Any(x => x.TypeName == typeName && x.TypeId != cateId))
                {
                    AppData.MainMV.TipMessage = "类型名称重复，请重新输入";
                    return;
                }

                int editType = 2;
                int categoryType = vm.IsCommonReply ? 1 : 2;

                var result = SDKClient.SDKClient.Instance.PostQuickReplyCategoryedit(editType, cateId, typeName, categoryType);
                if (!result.success)
                {
                    AppData.MainMV.TipMessage = "修改类型名失败";
                }
                else
                {
                    vm.SelectedType.TypeName = typeName;
                    AppData.MainMV.TipMessage = "修改类型名成功";
                }
            }
        }

        private void cmbReplyType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext is FastReplyViewModel vm)
            {
                if (vm.SelectedType != null && vm.SelectedType.TypeItems.Count == 0)
                {
                    this.AddEmptyContentItem();
                }
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.cmbReplyType.IsDropDownOpen = false;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (!Views.MessageBox.ShowDialogBox("删除该类型会将旗下所有的回复内容进行删除， 是/否？"))
            {
                return;
            }

            if (this.DataContext is FastReplyViewModel vm)
            {
                Button btn = sender as Button;
                FastReplyTypeModel model = btn.DataContext as FastReplyTypeModel;
                vm.TypeItems.Remove(model);

                string typeName = model.TypeName;
                int cateId = model.TypeId;
                int editType = 3;
                int categoryType = vm.IsCommonReply ? 1 : 2;

                var result = SDKClient.SDKClient.Instance.PostQuickReplyCategoryedit(editType, cateId, typeName, categoryType);
                if (!result.success)
                {
                    AppData.MainMV.TipMessage = "删除类型失败";
                }
                else
                {
                    AppData.MainMV.TipMessage = "删除类型成功";
                }
            }
        }
    }
}
