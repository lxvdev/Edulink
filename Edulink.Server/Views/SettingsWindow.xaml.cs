﻿using Edulink.ViewModels;
using System.Windows;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            SettingsWindowViewModel viewModel = new SettingsWindowViewModel();
            Closing += viewModel.OnWindowClosing;
            DataContext = viewModel;

            PortTextBox.Focus();
        }
    }
}
