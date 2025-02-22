using Edulink.ViewModels;
using System.Windows;

namespace Edulink.Views
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
            aboutDialogViewModel.RequestClose += (s, e) => Close();
            DataContext = aboutDialogViewModel;
        }
    }
}
