﻿#pragma checksum "..\..\..\..\Views\Panels\ChatHistoryView.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "5EBA4C0C8493A8C044648A3E3D6B417D4D9831BE7E792DE5DC91BF7FB8BC0A7B"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using IMClient.Converter;
using IMClient.Helper;
using IMClient.Views.Controls;
using IMCustomControls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace IMClient.Views.Panels {
    
    
    /// <summary>
    /// ChatHistoryView
    /// </summary>
    public partial class ChatHistoryView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 23 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DockPanel dp_TypesCheck;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton rbtnAll;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal IMCustomControls.PopupToggleButton pbtnMore;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal IMCustomControls.PopupToggleButton ptbtnCalendar;
        
        #line default
        #line hidden
        
        
        #line 35 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.Popup ppMore;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.Popup ppCalendar;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Calendar calendar;
        
        #line default
        #line hidden
        
        
        #line 164 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer sv;
        
        #line default
        #line hidden
        
        
        #line 165 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RichTextBox rictBox;
        
        #line default
        #line hidden
        
        
        #line 168 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ContextMenu cmMenu;
        
        #line default
        #line hidden
        
        
        #line 173 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal IMClient.Helper.FlowDocumentEx flowDoc;
        
        #line default
        #line hidden
        
        
        #line 211 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grid_LoadWithoutContent;
        
        #line default
        #line hidden
        
        
        #line 232 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grid_SearchWithoutContent;
        
        #line default
        #line hidden
        
        
        #line 251 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_BackToAll;
        
        #line default
        #line hidden
        
        
        #line 254 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal IMClient.Views.Controls.AnimationLoading aniLoading;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/IMUI;component/views/panels/chathistoryview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.dp_TypesCheck = ((System.Windows.Controls.DockPanel)(target));
            return;
            case 2:
            this.rbtnAll = ((System.Windows.Controls.RadioButton)(target));
            
            #line 24 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
            this.rbtnAll.Checked += new System.Windows.RoutedEventHandler(this.Type_Checked);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 25 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
            ((System.Windows.Controls.RadioButton)(target)).Checked += new System.Windows.RoutedEventHandler(this.Type_Checked);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 26 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
            ((System.Windows.Controls.RadioButton)(target)).Checked += new System.Windows.RoutedEventHandler(this.Type_Checked);
            
            #line default
            #line hidden
            return;
            case 5:
            this.pbtnMore = ((IMCustomControls.PopupToggleButton)(target));
            return;
            case 6:
            this.ptbtnCalendar = ((IMCustomControls.PopupToggleButton)(target));
            return;
            case 7:
            this.ppMore = ((System.Windows.Controls.Primitives.Popup)(target));
            return;
            case 8:
            
            #line 37 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItem_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.ppCalendar = ((System.Windows.Controls.Primitives.Popup)(target));
            return;
            case 10:
            this.calendar = ((System.Windows.Controls.Calendar)(target));
            
            #line 42 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
            this.calendar.SelectedDatesChanged += new System.EventHandler<System.Windows.Controls.SelectionChangedEventArgs>(this.calendar_SelectedDatesChanged);
            
            #line default
            #line hidden
            
            #line 42 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
            this.calendar.AddHandler(System.Windows.ContentElement.PreviewMouseUpEvent, new System.Windows.Input.MouseButtonEventHandler(this.calendar_PreviewMouseUp));
            
            #line default
            #line hidden
            return;
            case 11:
            this.sv = ((System.Windows.Controls.ScrollViewer)(target));
            return;
            case 12:
            this.rictBox = ((System.Windows.Controls.RichTextBox)(target));
            return;
            case 13:
            this.cmMenu = ((System.Windows.Controls.ContextMenu)(target));
            return;
            case 14:
            this.flowDoc = ((IMClient.Helper.FlowDocumentEx)(target));
            return;
            case 15:
            this.grid_LoadWithoutContent = ((System.Windows.Controls.Grid)(target));
            return;
            case 16:
            this.grid_SearchWithoutContent = ((System.Windows.Controls.Grid)(target));
            return;
            case 17:
            this.btn_BackToAll = ((System.Windows.Controls.Button)(target));
            
            #line 251 "..\..\..\..\Views\Panels\ChatHistoryView.xaml"
            this.btn_BackToAll.Click += new System.Windows.RoutedEventHandler(this.btn_BackToAll_Click);
            
            #line default
            #line hidden
            return;
            case 18:
            this.aniLoading = ((IMClient.Views.Controls.AnimationLoading)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

