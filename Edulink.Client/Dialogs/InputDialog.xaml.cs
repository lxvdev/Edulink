using System.Windows;
using System.Windows.Input;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public string InputValue { get; private set; }
        public InputDialogResult Result { get; private set; }

        public InputDialog(string dialogHeader, string caption)
        {
            InitializeComponent();
            DialogText.Text = dialogHeader;
            Title = caption;
            InputTextBox.Focus();
        }

        private void SendInput()
        {
            InputValue = InputTextBox.Text;
            Result = InputDialogResult.Ok;
            DialogResult = true;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SendInput();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = InputDialogResult.Cancel;
            DialogResult = true;
            Close();
        }

        private void InputTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendInput();
            }
        }

        public static (InputDialogResult result, string input) Show(string dialogHeader, string caption)
        {
            InputDialog dialog = new InputDialog(dialogHeader, caption);
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                return (dialog.Result, dialog.InputValue);
            }

            return (InputDialogResult.None, null);
        }
    }

    public enum InputDialogResult
    {
        None,
        Ok,
        Cancel
    }
}