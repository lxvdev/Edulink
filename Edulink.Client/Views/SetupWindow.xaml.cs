using Edulink.ViewModels;
using System.Windows;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        public SetupWindow()
        {
            InitializeComponent();
            SetupWindowViewModel viewModel = new SetupWindowViewModel();
            viewModel.RequestClose += (sender, e) => Close();
            DataContext = viewModel;
        }
    }
}
