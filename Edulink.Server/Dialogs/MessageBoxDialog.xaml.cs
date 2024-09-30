using System.Windows;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for MessageBoxDialog.xaml
    /// </summary>
    public partial class MessageBoxDialog : Window
    {
        private ClientInfo _clientInfo;
        public MessageBoxDialog(string message, ClientInfo clientInfo)
        {
            InitializeComponent();
            _clientInfo = clientInfo;
            MessageContent.Text = message;
            Title = $"{Application.Current.Resources["MessageFrom"]} {clientInfo.Name}";
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void RespondButton_Click(object sender, RoutedEventArgs e)
        {
            InputDialog messageInputDialog = new InputDialog(
                        (string)Application.Current.Resources["SendMessageDialogContent"],
                        (string)Application.Current.Resources["SendMessageDialogCaption"]);

            if (messageInputDialog.ShowDialog() == true && _clientInfo != null)
            {
                await _clientInfo.Helper.SendCommandAsync("SendMessage", messageInputDialog.InputValue);
            }
            Close();
        }
    }
}
