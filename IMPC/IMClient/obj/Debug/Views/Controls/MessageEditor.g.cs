﻿#pragma checksum "..\..\..\..\Views\Controls\MessageEditor.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "F456435C3CA44AF17731A9AD10BA719A8253CB739E3E2CF8BC87BDEC18C2898B"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using IM.Emoje;
using IMClient.Converter;
using IMCustomControls;
using IMCustomControls.Controls;
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
using System.Windows.Interactivity;
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


namespace IMClient.Views.Controls {
    
    
    /// <summary>
    /// MessageEditor
    /// </summary>
    public partial class MessageEditor : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 29 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.Popup ppEmoje;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border bdEmoje;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal IMCustomControls.Controls.PopupEx ppMember;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox listMember;
        
        #line default
        #line hidden
        
        
        #line 76 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal IMCustomControls.PopupToggleButton btnEmoji;
        
        #line default
        #line hidden
        
        
        #line 78 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnImage;
        
        #line default
        #line hidden
        
        
        #line 79 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnClip;
        
        #line default
        #line hidden
        
        
        #line 80 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnFile;
        
        #line default
        #line hidden
        
        
        #line 83 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSharePersonCard;
        
        #line default
        #line hidden
        
        
        #line 88 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RichTextBox richBox;
        
        #line default
        #line hidden
        
        
        #line 102 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ContextMenu cmMenu;
        
        #line default
        #line hidden
        
        
        #line 103 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem miCut;
        
        #line default
        #line hidden
        
        
        #line 104 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem miCopy;
        
        #line default
        #line hidden
        
        
        #line 105 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem miPaste;
        
        #line default
        #line hidden
        
        
        #line 106 "..\..\..\..\Views\Controls\MessageEditor.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem miImgSaveAs;
        
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
            System.Uri resourceLocater = new System.Uri("/IMUI;component/views/controls/messageeditor.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\Controls\MessageEditor.xaml"
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
            case 1:
            this.ppEmoje = ((System.Windows.Controls.Primitives.Popup)(target));
            return;
            case 2:
            this.bdEmoje = ((System.Windows.Controls.Border)(target));
            return;
            case 3:
            this.ppMember = ((IMCustomControls.Controls.PopupEx)(target));
            return;
            case 4:
            this.listMember = ((System.Windows.Controls.ListBox)(target));
            return;
            case 6:
            this.btnEmoji = ((IMCustomControls.PopupToggleButton)(target));
            return;
            case 7:
            this.btnImage = ((System.Windows.Controls.Button)(target));
            
            #line 78 "..\..\..\..\Views\Controls\MessageEditor.xaml"
            this.btnImage.Click += new System.Windows.RoutedEventHandler(this.btnImage_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.btnClip = ((System.Windows.Controls.Button)(target));
            
            #line 79 "..\..\..\..\Views\Controls\MessageEditor.xaml"
            this.btnClip.Click += new System.Windows.RoutedEventHandler(this.btnClip_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.btnFile = ((System.Windows.Controls.Button)(target));
            
            #line 80 "..\..\..\..\Views\Controls\MessageEditor.xaml"
            this.btnFile.Click += new System.Windows.RoutedEventHandler(this.btnFile_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.btnSharePersonCard = ((System.Windows.Controls.Button)(target));
            
            #line 83 "..\..\..\..\Views\Controls\MessageEditor.xaml"
            this.btnSharePersonCard.Click += new System.Windows.RoutedEventHandler(this.btnSharePersonCard_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.richBox = ((System.Windows.Controls.RichTextBox)(target));
            return;
            case 12:
            this.cmMenu = ((System.Windows.Controls.ContextMenu)(target));
            
            #line 102 "..\..\..\..\Views\Controls\MessageEditor.xaml"
            this.cmMenu.Opened += new System.Windows.RoutedEventHandler(this.ContextMenu_Opened);
            
            #line default
            #line hidden
            
            #line 102 "..\..\..\..\Views\Controls\MessageEditor.xaml"
            this.cmMenu.AddHandler(System.Windows.Controls.MenuItem.ClickEvent, new System.Windows.RoutedEventHandler(this.ContextMenu_Click));
            
            #line default
            #line hidden
            return;
            case 13:
            this.miCut = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 14:
            this.miCopy = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 15:
            this.miPaste = ((System.Windows.Controls.MenuItem)(target));
            return;
            case 16:
            this.miImgSaveAs = ((System.Windows.Controls.MenuItem)(target));
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
            case 5:
            
            #line 53 "..\..\..\..\Views\Controls\MessageEditor.xaml"
            ((System.Windows.Controls.Grid)(target)).MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.grid_MouseLeftButtonDown);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}
