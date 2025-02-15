using Edulink.Models;
using Edulink.ViewModels;
using System.Drawing;
using System.Windows;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for DesktopPreviewDialog.xaml
    /// </summary>
    public partial class DesktopPreviewDialog : Window
    {
        private DesktopPreviewDialogViewModel _viewModel;

        public Client Client => _viewModel.Client;

        public DesktopPreviewDialog(Client client)
        {
            InitializeComponent();
            _viewModel = new DesktopPreviewDialogViewModel(client);
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
