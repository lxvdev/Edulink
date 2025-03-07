using Edulink.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for PasswordDialog.xaml
    /// </summary>
    public partial class PasswordDialog : Window
    {
        private PasswordDialogViewModel _viewModel;

        public PasswordDialog(PasswordDialogType type)
        {
            InitializeComponent();
            _viewModel = new PasswordDialogViewModel(type);
            _viewModel.RequestClose += OnRequestClose;
            DataContext = _viewModel;

            CurrentPasswordBox.Focus();
        }

        private void OnRequestClose(object sender, bool dialogResult)
        {
            if (sender is PasswordDialogViewModel viewModel)
            {
                DialogResult = dialogResult;
                Close();
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = PasswordBox.Password;
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.CurrentPassword = CurrentPasswordBox.Password;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!(e.Key == Key.Enter))
                return;

            if (_viewModel != null)
            {
                if (!_viewModel.ConfirmCommand.CanExecute(null))
                    return;

                _viewModel.ConfirmCommand.Execute(null);
            }
        }

        public static bool Show(PasswordDialogType type)
        {
            if (type == PasswordDialogType.EnterPassword && string.IsNullOrEmpty(App.SettingsManager.Settings.Password))
            {
                MessageDialogResult userChoice = MessageDialog.ShowLocalized("Message.Content.NoPasswordSet", MessageDialogTitle.Warning, MessageDialogButton.YesNo, MessageDialogIcon.Warning);

                if (userChoice.ButtonResult == MessageDialogButtonResult.No)
                {
                    return true;
                }
                else if (userChoice.ButtonResult == MessageDialogButtonResult.None)
                {
                    return false;
                }
            }

            PasswordDialog dialog = new PasswordDialog(type);
            return dialog.ShowDialog() == true && dialog.DialogResult == true;
        }
    }

    public enum PasswordDialogType
    {
        EnterPassword,
        SetOrChangePassword,
    }
}
