using Edulink.Models;
using Edulink.ViewModels;
using System;
using System.IO;
using System.Windows;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for ReceiveFileWindow.xaml
    /// </summary>
    public partial class ReceiveFileWindow : Window
    {
        private ReceiveFileWindowViewModel _viewModel;

        public ReceiveFileWindow(Computer sourceComputer, FileInfo fileInfo)
        {
            InitializeComponent();
            _viewModel = new ReceiveFileWindowViewModel(sourceComputer, fileInfo);
            _viewModel.RequestAccept += _viewModel_RequestAccept;
            DataContext = _viewModel;
        }

        private void _viewModel_RequestAccept(object sender, EventArgs e)
        {
            // TODO: Handle accept
        }
    }
}
