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

        public InputDialog(string dialogText, string caption)
        {
            InitializeComponent();
            DialogText.Text = dialogText;
            Title = caption;
            InputTextBox.Focus();
        }

        private void SendInput()
        {
            InputValue = InputTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SendInput();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void InputTextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendInput();
            }
        }
    }
}
