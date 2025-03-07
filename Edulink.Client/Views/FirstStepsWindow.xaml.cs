using Edulink.ViewModels;
using System.Windows;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for FirstStepsWindow.xaml
    /// </summary>
    public partial class FirstStepsWindow : Window
    {
        public FirstStepsWindow()
        {
            InitializeComponent();
            FirstStepsWindowViewModel viewModel = new FirstStepsWindowViewModel();
            viewModel.RequestClose += (sender, e) => Close();
            DataContext = viewModel;
        }
    }
}
