using Edulink.Client;
using System.Windows;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for MessageBoxDialog.xaml
    /// </summary>
    public partial class MessageBoxDialog : Window
    {
        public MessageBoxDialog(string message)
        {
            InitializeComponent();
            MessageContent.Text = message;
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

            if (messageInputDialog.ShowDialog() == true)
            {
                await App.client.helper.SendCommandAsync("SendMessage", messageInputDialog.InputValue);
            }
            Close();
        }
    }
}
