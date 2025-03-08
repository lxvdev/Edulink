using Edulink.Classes;
using Edulink.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialogButtonResult ButtonResult { get; private set; }
        public string InputValue { get; private set; }

        private InputDialogViewModel _viewModel;

        public InputDialog(string message, string title)
        {
            InitializeComponent();
            _viewModel = new InputDialogViewModel(message, title);
            _viewModel.RequestClose += OnRequestClose;
            DataContext = _viewModel;

            InputTextBox.Focus();
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!(e.Key == Key.Enter))
                return;

            if (_viewModel != null)
            {
                if (!_viewModel.OkCommand.CanExecute(null))
                    return;

                _viewModel.OkCommand.Execute(null);
            }
        }

        private void OnRequestClose(object sender, bool dialogResult)
        {
            if (sender is InputDialogViewModel viewModel)
            {
                ButtonResult = viewModel.ButtonResult;
                InputValue = viewModel.InputValue;
                DialogResult = dialogResult;
                Close();
            }
        }

        public static InputDialogResult Show(string message, string title = null)
        {
            InputDialog dialog = new InputDialog(message, title);
            if (dialog.ShowDialog() == true)
            {
                return new InputDialogResult(dialog.ButtonResult, dialog.InputValue);
            }

            return new InputDialogResult(dialog.ButtonResult);
        }

        public static InputDialogResult ShowLocalized(string messageKey, string title = null)
        {
            string message = LocalizedStrings.Instance[messageKey];
            return Show(message, title);
        }
    }

    public class InputDialogResult
    {
        public InputDialogButtonResult ButtonResult { get; set; }
        public string InputResult { get; set; }

        public InputDialogResult(InputDialogButtonResult buttonResult, string reply = null)
        {
            ButtonResult = buttonResult;
            InputResult = reply;
        }
    }

    public enum InputDialogButtonResult
    {
        None,
        Ok,
        Cancel
    }
}