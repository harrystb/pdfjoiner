﻿using System.Windows;

namespace pdfjoiner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var viewModel = new MainViewModel();
            DataContext = viewModel;

            InitializeComponent();

        }
    }
}
