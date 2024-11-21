using Edulink.Controls.MaterialSymbol;
using System.Windows;
using System.Windows.Media;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for MessageBoxDialog.xaml
    /// </summary>
    public partial class MessageBoxDialog : Window
    {
        public MessageBoxDialogResult Result { get; private set; } = MessageBoxDialogResult.None;

        public MessageBoxDialog(string message, string title = "", MessageBoxDialogButtons buttons = MessageBoxDialogButtons.Ok, MessageBoxDialogType type = MessageBoxDialogType.None)
        {
            InitializeComponent();
            ConfigureMessage(message, title, type);
            ConfigureButtons(buttons);
        }

        public void ConfigureMessage(string message, string title, MessageBoxDialogType type)
        {
            MessageContent.Text = message;
            SetMessageType(type);
            Title = string.IsNullOrEmpty(title) ? Title : title;
        }

        private void SetMessageType(MessageBoxDialogType type)
        {
            switch (type)
            {
                case MessageBoxDialogType.Error:
                    Title = (string)Application.Current.Resources["Message.TitleBar.Error"];
                    Symbol.Kind = MaterialSymbolKind.Error;
                    Symbol.SymbolBrush = Brushes.Red;
                    Symbol.Visibility = Visibility.Visible;
                    break;
                case MessageBoxDialogType.Information:
                    Title = (string)Application.Current.Resources["Message.TitleBar.Information"];
                    Symbol.Kind = MaterialSymbolKind.Info;
                    Symbol.SymbolBrush = Brushes.RoyalBlue;
                    Symbol.Visibility = Visibility.Visible;
                    break;
                case MessageBoxDialogType.Warning:
                    Title = (string)Application.Current.Resources["Message.TitleBar.Warning"];
                    Symbol.Kind = MaterialSymbolKind.Warning;
                    Symbol.SymbolBrush = Brushes.Orange;
                    Symbol.Visibility = Visibility.Visible;
                    break;
                case MessageBoxDialogType.Success:
                    Title = (string)Application.Current.Resources["Message.TitleBar.Success"];
                    Symbol.Kind = MaterialSymbolKind.CheckCircle;
                    Symbol.SymbolBrush = Brushes.Green;
                    Symbol.Visibility = Visibility.Visible;
                    break;
                default:
                    Symbol.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void ConfigureButtons(MessageBoxDialogButtons buttons)
        {
            switch (buttons)
            {
                case MessageBoxDialogButtons.Ok:
                    OkButton.Visibility = Visibility.Visible;
                    OkButton.Click += OkButton_Click;
                    break;
                case MessageBoxDialogButtons.Reply:
                    ReplyButton.Visibility = Visibility.Visible;
                    ReplyButton.Click += ReplyButton_Click;
                    break;
                case MessageBoxDialogButtons.Cancel:
                    CancelButton.Visibility = Visibility.Visible;
                    CancelButton.Click += CancelButton_Click;
                    break;
                case MessageBoxDialogButtons.OkAndCancel:
                    OkButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    OkButton.Click += OkButton_Click;
                    CancelButton.Click += CancelButton_Click;
                    break;
                case MessageBoxDialogButtons.OkAndReply:
                    OkButton.Visibility = Visibility.Visible;
                    ReplyButton.Visibility = Visibility.Visible;
                    OkButton.Click += OkButton_Click;
                    ReplyButton.Click += ReplyButton_Click;
                    break;
                case MessageBoxDialogButtons.YesNo:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    YesButton.Click += YesButton_Click;
                    NoButton.Click += NoButton_Click;
                    break;
                default:
                    break;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxDialogResult.Ok;
            DialogResult = true;
            Close();
        }

        private void ReplyButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxDialogResult.Reply;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxDialogResult.Cancel;
            DialogResult = true;
            Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxDialogResult.Yes;
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxDialogResult.No;
            DialogResult = true;
            Close();
        }

        public static MessageBoxDialogResult Show(string message, string title = "", MessageBoxDialogButtons buttons = MessageBoxDialogButtons.Ok, MessageBoxDialogType type = MessageBoxDialogType.None)
        {
            MessageBoxDialog dialog = new MessageBoxDialog(message, title, buttons, type);
            if (dialog.ShowDialog() == true)
            {
                return dialog.Result;

            }
            else { return MessageBoxDialogResult.None; }
        }
    }

    public enum MessageBoxDialogResult
    {
        None,
        Ok,
        Cancel,
        Reply,
        Yes,
        No
    }

    public enum MessageBoxDialogType
    {
        None,
        Error,
        Information,
        Warning,
        Success
    }

    public enum MessageBoxDialogButtons
    {
        None,
        Ok,
        Reply,
        Cancel,
        OkAndCancel,
        OkAndReply,
        YesNo
    }
}