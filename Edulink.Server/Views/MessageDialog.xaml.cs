using Edulink.ViewModels;
using System.Windows;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for MessageBoxDialog.xaml
    /// </summary>
    public partial class MessageDialog : Window
    {
        public MessageDialogButtonResult ButtonResult { get; private set; }
        public string ReplyResult { get; private set; }

        public MessageDialog(string message, string title = null, MessageDialogButton button = MessageDialogButton.Ok, MessageDialogIcon icon = MessageDialogIcon.None)
        {
            InitializeComponent();
            MessageDialogViewModel viewModel = new MessageDialogViewModel(message, title, button, icon);
            viewModel.RequestClose += OnRequestClose;
            DataContext = viewModel;
        }

        private void OnRequestClose(object sender, bool dialogResult)
        {
            if (sender is MessageDialogViewModel viewModel)
            {
                ButtonResult = viewModel.ButtonResult;
                ReplyResult = viewModel.ReplyResult;
                DialogResult = dialogResult;
                Close();
            }
        }

        public static MessageDialogResult Show(string message, string title = null, MessageDialogButton button = MessageDialogButton.Ok, MessageDialogIcon icon = MessageDialogIcon.None)
        {
            MessageDialog dialog = new MessageDialog(message, title, button, icon);
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

        public MessageDialogResult(MessageDialogButtonResult buttonResult, string replyResult = null)
        {
            ButtonResult = buttonResult;
            ReplyResult = replyResult;
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

    public enum MessageDialogIcon
    {
        None,
        Error,
        Information,
        Warning,
        Success
    }

    public enum MessageDialogButton
    {
        None,
        Ok,
        Reply,
        Cancel,
        OkCancel,
        OkReply,
        YesNo
    }

    public static class MessageDialogTitle
    {
        public static string Error => (string)Application.Current.TryFindResource("Message.Title.Error");
        public static string Information => (string)Application.Current.TryFindResource("Message.Title.Information");
        public static string Warning => (string)Application.Current.TryFindResource("Message.Title.Warning");
        public static string Success => (string)Application.Current.TryFindResource("Message.Title.Success");
    }
}