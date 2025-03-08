using Edulink.Models;
using Edulink.ViewModels;
using System.Windows;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for UpdateDialog.xaml
    /// </summary>
    public partial class UpdaterDialog : Window
    {
        public UpdaterDialog(ReleaseDetails updateDetails = null, bool startUpdate = false)
        {
            InitializeComponent();

            UpdaterDialogViewModel viewModel;
            if (updateDetails == null)
            {
                viewModel = new UpdaterDialogViewModel();
            }
            else
            {
                viewModel = new UpdaterDialogViewModel(updateDetails);
            }
            viewModel.RequestClose += (sender, e) => Close();
            DataContext = viewModel;
            if (startUpdate)
            {
                if (viewModel.UpdateCommand.CanExecute(null))
                {
                    viewModel.UpdateCommand.Execute(null);
                }
                else { Close(); }
            }
        }
    }
}
