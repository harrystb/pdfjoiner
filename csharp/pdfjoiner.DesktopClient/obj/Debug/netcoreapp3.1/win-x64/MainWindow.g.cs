﻿#pragma checksum "..\..\..\..\MainWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "3A17230C25E4B5717F17D77F7EC2E424F24730E2"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
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
using pdfjoiner.Core.Models;
using pdfjoiner.DesktopClient;


namespace pdfjoiner.DesktopClient {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 47 "..\..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BrowseFileButton;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BrowseFolderButton;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ResetButton;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TreeView FolderView;
        
        #line default
        #line hidden
        
        
        #line 88 "..\..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AddPagesButton;
        
        #line default
        #line hidden
        
        
        #line 95 "..\..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox TitleTextBox;
        
        #line default
        #line hidden
        
        
        #line 98 "..\..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox PathTextBox;
        
        #line default
        #line hidden
        
        
        #line 111 "..\..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox StartIndexTextBox;
        
        #line default
        #line hidden
        
        
        #line 119 "..\..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox EndIndexTexBox;
        
        #line default
        #line hidden
        
        
        #line 152 "..\..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button GenerateButton;
        
        #line default
        #line hidden
        
        
        #line 155 "..\..\..\..\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ClearSegmentsButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.8.1.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/pdfjoiner;V1.0.0.0;component/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.8.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 13 "..\..\..\..\MainWindow.xaml"
            ((System.Windows.Controls.Grid)(target)).AddHandler(System.Windows.DragDrop.DropEvent, new System.Windows.DragEventHandler(this.DragDropEventHandler));
            
            #line default
            #line hidden
            return;
            case 2:
            this.BrowseFileButton = ((System.Windows.Controls.Button)(target));
            return;
            case 3:
            this.BrowseFolderButton = ((System.Windows.Controls.Button)(target));
            return;
            case 4:
            this.ResetButton = ((System.Windows.Controls.Button)(target));
            return;
            case 5:
            this.FolderView = ((System.Windows.Controls.TreeView)(target));
            
            #line 57 "..\..\..\..\MainWindow.xaml"
            this.FolderView.SelectedItemChanged += new System.Windows.RoutedPropertyChangedEventHandler<object>(this.SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.AddPagesButton = ((System.Windows.Controls.Button)(target));
            return;
            case 7:
            this.TitleTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            this.PathTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.StartIndexTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 115 "..\..\..\..\MainWindow.xaml"
            this.StartIndexTextBox.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.LessThanMaxPage);
            
            #line default
            #line hidden
            return;
            case 10:
            this.EndIndexTexBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 123 "..\..\..\..\MainWindow.xaml"
            this.EndIndexTexBox.PreviewTextInput += new System.Windows.Input.TextCompositionEventHandler(this.LessThanMaxPage);
            
            #line default
            #line hidden
            return;
            case 11:
            this.GenerateButton = ((System.Windows.Controls.Button)(target));
            return;
            case 12:
            this.ClearSegmentsButton = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

