﻿#pragma checksum "..\..\..\..\Views\Panels\SetFastReply.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "6E8C1DD66B18F1946D9A64C3F9DEA829052CB1A62D0BE195838316E4D06DD8EC"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using CSClient.Views.Panels;
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


namespace CSClient.Views.Panels {
    
    
    /// <summary>
    /// SetFastReply
    /// </summary>
    public partial class SetFastReply : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 204 "..\..\..\..\Views\Panels\SetFastReply.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbReplyType;
        
        #line default
        #line hidden
        
        
        #line 207 "..\..\..\..\Views\Panels\SetFastReply.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtAddTypeName;
        
        #line default
        #line hidden
        
        
        #line 208 "..\..\..\..\Views\Panels\SetFastReply.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnAddType;
        
        #line default
        #line hidden
        
        
        #line 213 "..\..\..\..\Views\Panels\SetFastReply.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtEditTypeName;
        
        #line default
        #line hidden
        
        
        #line 214 "..\..\..\..\Views\Panels\SetFastReply.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnEditType;
        
        #line default
        #line hidden
        
        
        #line 223 "..\..\..\..\Views\Panels\SetFastReply.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox list;
        
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
            System.Uri resourceLocater = new System.Uri("/CSClient;component/views/panels/setfastreply.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\Panels\SetFastReply.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
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
            case 5:
            this.cmbReplyType = ((System.Windows.Controls.ComboBox)(target));
            
            #line 204 "..\..\..\..\Views\Panels\SetFastReply.xaml"
            this.cmbReplyType.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cmbReplyType_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.txtAddTypeName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.btnAddType = ((System.Windows.Controls.Button)(target));
            
            #line 208 "..\..\..\..\Views\Panels\SetFastReply.xaml"
            this.btnAddType.Click += new System.Windows.RoutedEventHandler(this.btnAddType_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.txtEditTypeName = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.btnEditType = ((System.Windows.Controls.Button)(target));
            
            #line 214 "..\..\..\..\Views\Panels\SetFastReply.xaml"
            this.btnEditType.Click += new System.Windows.RoutedEventHandler(this.btnEditType_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.list = ((System.Windows.Controls.ListBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 51 "..\..\..\..\Views\Panels\SetFastReply.xaml"
            ((System.Windows.Controls.Grid)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.gridAdd_MouseLeftButtonUp);
            
            #line default
            #line hidden
            break;
            case 2:
            
            #line 54 "..\..\..\..\Views\Panels\SetFastReply.xaml"
            ((System.Windows.Controls.Grid)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.gridRemove_MouseLeftButtonUp);
            
            #line default
            #line hidden
            break;
            case 3:
            
            #line 82 "..\..\..\..\Views\Panels\SetFastReply.xaml"
            ((System.Windows.Controls.Border)(target)).MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Border_MouseLeftButtonDown);
            
            #line default
            #line hidden
            break;
            case 4:
            
            #line 84 "..\..\..\..\Views\Panels\SetFastReply.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.btnRemove_Click);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}

