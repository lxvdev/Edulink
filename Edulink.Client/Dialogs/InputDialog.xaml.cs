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
        public InputDialogButtonResult Button { get; private set; }

        public InputDialog(string message, string title)
        {
            InitializeComponent();
            DialogText.Text = message;
            Title = title;
            InputTextBox.Focus();
        }

        private void SendInput()
        {
            InputValue = InputTextBox.Text;
            Button = InputDialogButtonResult.Ok;
            DialogResult = true;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SendInput();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Button = InputDialogButtonResult.Cancel;
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

        public static InputDialogResult Show(string message, string title)
        {
            InputDialog dialog = new InputDialog(message, title);
            if (dialog.ShowDialog() == true)
            {
                return new InputDialogResult(dialog.Button, dialog.InputValue);
            }

            return new InputDialogResult(dialog.Button);
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

        public static bool operator ==(InputDialogResult result, InputDialogButtonResult buttonResult)
        {
            return result != null && result.ButtonResult == buttonResult;
        }

        public static bool operator !=(InputDialogResult result, InputDialogButtonResult buttonResult)
        {
            return !(result == buttonResult);
        }

        public override bool Equals(object obj)
        {
            if (obj is InputDialogResult result)
            {
                return this.ButtonResult == result.ButtonResult;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ButtonResult.GetHashCode();
        }
    }

    public enum InputDialogButtonResult
    {
        None,
        Ok,
        Cancel
    }
}