using Edulink.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialogButtonResult ButtonResult { get; private set; }
        public string InputValue { get; private set; }

        public InputDialog(string message, string title)
        {
            InitializeComponent();
            InputDialogViewModel inputDialogViewModel = new InputDialogViewModel(message, title);
            inputDialogViewModel.RequestClose += OnRequestClose;
            DataContext = inputDialogViewModel;

            InputTextBox.Focus();
        }

        private void InputTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!(e.Key == Key.Enter))
                return;

            if (DataContext is InputDialogViewModel viewModel)
            {
                if (!viewModel.OkCommand.CanExecute(null))
                    return;

                viewModel.OkCommand.Execute(null);
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

        public static InputDialogResult Show(string message, string title)
        {
            InputDialog dialog = new InputDialog(message, title);
            if (dialog.ShowDialog() == true)
            {
                return new InputDialogResult(dialog.ButtonResult, dialog.InputValue);
            }

            return new InputDialogResult(dialog.ButtonResult);
        }
    }

    public class InputDialogResult
    {
        public InputDialogButtonResult ButtonResult { get; set; }
        public string ReplyResult { get; set; }

        public InputDialogResult(InputDialogButtonResult buttonResult, string reply = null)
        {
            ButtonResult = buttonResult;
            ReplyResult = reply;
        }
    }

    public enum InputDialogButtonResult
    {
        None,
        Ok,
        Cancel
    }
}