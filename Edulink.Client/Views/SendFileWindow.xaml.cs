using Edulink.Models;
using Edulink.ViewModels;
using System.Collections.Generic;
using System.Windows;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for SendFileWindow.xaml
    /// </summary>
    public partial class SendFileWindow : Window
    {
        private SendFileWindowViewModel _viewModel;

        public SendFileWindow()
        {
            InitializeComponent();
            _viewModel = new SendFileWindowViewModel();
            DataContext = _viewModel;
        }

        public void UpdateList(List<Computer> computerList)
        {
            _viewModel.UpdateList(computerList);
        }

        public void SetSharingStatus(bool enabled)
        {
            _viewModel.SetSharingStatus(enabled);
        }
    }
}
