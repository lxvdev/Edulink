using Edulink.Controls.MaterialSymbol;
using System.Windows;
using System.Windows.Media;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for MessageBoxDialog.xaml
    /// </summary>
    public partial class MessageDialog : Window
    {
        public MessageDialogButtonResult ButtonResult { get; private set; } = MessageDialogButtonResult.None;
        public string ReplyResult { get; private set; }

        public MessageDialog(string message, string title = "", MessageDialogButtons buttons = MessageDialogButtons.Ok, MessageDialogType type = MessageDialogType.None)
        {
            InitializeComponent();
            ConfigureMessage(message, title, type);
            ConfigureButtons(buttons);
        }

        public void ConfigureMessage(string message, string title, MessageDialogType type)
        {
            MessageContent.Text = message;
            SetMessageType(type);
            Title = string.IsNullOrEmpty(title) ? Title : title;
        }

        private void SetMessageType(MessageDialogType type)
        {
            switch (type)
            {
                case MessageDialogType.Error:
                    Title = (string)Application.Current.Resources["Message.TitleBar.Error"];
                    Symbol.Kind = MaterialSymbolKind.Error;
                    Symbol.SymbolBrush = Brushes.Red;
                    Symbol.Visibility = Visibility.Visible;
                    break;
                case MessageDialogType.Information:
                    Title = (string)Application.Current.Resources["Message.TitleBar.Information"];
                    Symbol.Kind = MaterialSymbolKind.Info;
                    Symbol.SymbolBrush = Brushes.RoyalBlue;
                    Symbol.Visibility = Visibility.Visible;
                    break;
                case MessageDialogType.Warning:
                    Title = (string)Application.Current.Resources["Message.TitleBar.Warning"];
                    Symbol.Kind = MaterialSymbolKind.Warning;
                    Symbol.SymbolBrush = Brushes.Orange;
                    Symbol.Visibility = Visibility.Visible;
                    break;
                case MessageDialogType.Success:
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

        private void ConfigureButtons(MessageDialogButtons buttons)
        {
            switch (buttons)
            {
                case MessageDialogButtons.Ok:
                    OkButton.Visibility = Visibility.Visible;
                    OkButton.Click += OkButton_Click;
                    break;
                case MessageDialogButtons.Reply:
                    ReplyButton.Visibility = Visibility.Visible;
                    ReplyButton.Click += ReplyButton_Click;
                    break;
                case MessageDialogButtons.Cancel:
                    CancelButton.Visibility = Visibility.Visible;
                    CancelButton.Click += CancelButton_Click;
                    break;
                case MessageDialogButtons.OkCancel:
                    OkButton.Visibility = Visibility.Visible;
                    CancelButton.Visibility = Visibility.Visible;
                    OkButton.Click += OkButton_Click;
                    CancelButton.Click += CancelButton_Click;
                    break;
                case MessageDialogButtons.OkReply:
                    OkButton.Visibility = Visibility.Visible;
                    ReplyButton.Visibility = Visibility.Visible;
                    OkButton.Click += OkButton_Click;
                    ReplyButton.Click += ReplyButton_Click;
                    break;
                case MessageDialogButtons.YesNo:
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
            ButtonResult = MessageDialogButtonResult.Ok;
            DialogResult = true;
            Close();
        }

        private void ReplyButton_Click(object sender, RoutedEventArgs e)
        {
            InputDialogResult result = InputDialog.Show((string)Application.Current.TryFindResource("Input.Content.SendMessage"), (string)Application.Current.TryFindResource("Input.TitleBar.SendMessage"));
            if (result == InputDialogButtonResult.Ok && !string.IsNullOrEmpty(result.ReplyResult))
            {
                ReplyResult = result.ReplyResult;
                ButtonResult = MessageDialogButtonResult.Reply;
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonResult = MessageDialogButtonResult.Cancel;
            DialogResult = true;
            Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonResult = MessageDialogButtonResult.Yes;
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonResult = MessageDialogButtonResult.No;
            DialogResult = true;
            Close();
        }

        public static MessageDialogResult Show(string message, string title = "", MessageDialogButtons buttons = MessageDialogButtons.Ok, MessageDialogType type = MessageDialogType.None)
        {
            MessageDialog dialog = new MessageDialog(message, title, buttons, type);
            if (dialog.ShowDialog() == true)
            {
                return new MessageDialogResult(dialog.ButtonResult, dialog.ReplyResult);

            }
            else { return new MessageDialogResult(MessageDialogButtonResult.None); }
        }
    }

    public class MessageDialogResult
    {
        public MessageDialogButtonResult ButtonResult { get; set; }
        public string ReplyResult { get; set; }

        public MessageDialogResult(MessageDialogButtonResult buttonResult, string reply = null)
        {
            ButtonResult = buttonResult;
            ReplyResult = reply;
        }

        public static bool operator ==(MessageDialogResult result, MessageDialogButtonResult buttonResult)
        {
            return result != null && result.ButtonResult == buttonResult;
        }

        public static bool operator !=(MessageDialogResult result, MessageDialogButtonResult buttonResult)
        {
            return !(result == buttonResult);
        }

        public override bool Equals(object obj)
        {
            if (obj is MessageDialogResult result)
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

    public enum MessageDialogButtonResult
    {
        None,
        Ok,
        Cancel,
        Reply,
        Yes,
        No
    }

    public enum MessageDialogType
    {
        None,
        Error,
        Information,
        Warning,
        Success
    }

    public enum MessageDialogButtons
    {
        None,
        Ok,
        Reply,
        Cancel,
        OkCancel,
        OkReply,
        YesNo
    }
}