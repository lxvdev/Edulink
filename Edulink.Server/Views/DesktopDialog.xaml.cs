using Edulink.Models;
using Edulink.ViewModels;
using System.Drawing;
using System.Windows;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for DesktopPreviewDialog.xaml
    /// </summary>
    public partial class DesktopDialog : Window
    {
        private DesktopDialogViewModel _viewModel;

        public Client Client => _viewModel.Client;

        public DesktopDialog(Client client)
        {
            InitializeComponent();
            _viewModel = new DesktopDialogViewModel(client);
            _viewModel.RequestClose += (s, e) => Close();
            _viewModel.RequestFocus += (s, e) => Focus();
            DataContext = _viewModel;
        }

        public void UpdateDesktop(Bitmap bitmap)
        {
            if (_viewModel != null)
            {
                _viewModel.UpdateScreenshot(bitmap);
            }
        }
    }
}
