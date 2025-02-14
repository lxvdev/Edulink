using Edulink.ViewModels;
using System;
using System.Windows;
namespace Edulink
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
            AboutDialogViewModel aboutDialogViewModel = new AboutDialogViewModel();
            aboutDialogViewModel.RequestClose += OnRequestClose;
            DataContext = aboutDialogViewModel;
        }

        private void OnRequestClose(object sender, EventArgs e)
        {
            if (sender is AboutDialogViewModel viewModel)
            {
                Close();
            }
        }
    }
}
