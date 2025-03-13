using Edulink.Classes;
using Edulink.MVVM;
using Edulink.Views;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Edulink.ViewModels
{
    public class MessageDialogViewModel : ClosableViewModel
    {
        public string Message { get; set; }
        public string Title { get; set; }

        public PackIconKind IconKind { get; set; } = PackIconKind.None;
        public SolidColorBrush IconColor { get; set; } = new SolidColorBrush(Colors.Black);
        public Visibility IconVisibility { get; set; } = Visibility.Collapsed;

        public Visibility OkButtonVisibility { get; set; } = Visibility.Collapsed;
        public Visibility RetryButtonVisibility { get; set; } = Visibility.Collapsed;
        public Visibility CancelButtonVisibility { get; set; } = Visibility.Collapsed;
        public Visibility ReplyButtonVisibility { get; set; } = Visibility.Collapsed;
        public Visibility YesButtonVisibility { get; set; } = Visibility.Collapsed;
        public Visibility NoButtonVisibility { get; set; } = Visibility.Collapsed;

        public MessageDialogButtonResult ButtonResult { get; set; }
        public string ReplyResult { get; set; }

        public MessageDialogViewModel(string message, string title, MessageDialogButton button, MessageDialogIcon icon)
        {
            Message = message;
            Title = title;
            SetMessageIcon(icon);
            SetButtonVisibility(button);
        }

        # region Icon and Button Setup
        public void SetMessageIcon(MessageDialogIcon icon)
        {
            switch (icon)
            {
                case MessageDialogIcon.Error:
                    IconKind = PackIconKind.Error;
                    IconColor = new SolidColorBrush(Colors.Red);
                    IconVisibility = Visibility.Visible;
                    break;
                case MessageDialogIcon.Information:
                    IconKind = PackIconKind.Info;
                    IconColor = new SolidColorBrush(Colors.RoyalBlue);
                    IconVisibility = Visibility.Visible;
                    break;
                case MessageDialogIcon.Warning:
                    IconKind = PackIconKind.Warning;
                    IconColor = new SolidColorBrush(Colors.Orange);
                    IconVisibility = Visibility.Visible;
                    break;
                case MessageDialogIcon.Success:
                    IconKind = PackIconKind.CheckCircle;
                    IconColor = new SolidColorBrush(Colors.Green);
                    IconVisibility = Visibility.Visible;
                    break;
                default:
                    IconVisibility = Visibility.Collapsed;
                    break;
            }
        }

        public void SetButtonVisibility(MessageDialogButton button)
        {
            switch (button)
            {
                case MessageDialogButton.Ok:
                    OkButtonVisibility = Visibility.Visible;
                    break;
                case MessageDialogButton.Reply:
                    ReplyButtonVisibility = Visibility.Visible;
                    break;
                case MessageDialogButton.Cancel:
                    CancelButtonVisibility = Visibility.Visible;
                    break;
                case MessageDialogButton.RetryCancel:
                    RetryButtonVisibility = Visibility.Visible;
                    CancelButtonVisibility = Visibility.Visible;
                    break;
                case MessageDialogButton.YesNo:
                    YesButtonVisibility = Visibility.Visible;
                    NoButtonVisibility = Visibility.Visible;
                    break;
                case MessageDialogButton.OkCancel:
                    OkButtonVisibility = Visibility.Visible;
                    CancelButtonVisibility = Visibility.Visible;
                    break;
                case MessageDialogButton.OkReply:
                    OkButtonVisibility = Visibility.Visible;
                    ReplyButtonVisibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Commands
        public ICommand OkCommand => new RelayCommand(execute => Ok());
        private void Ok()
        {
            ButtonResult = MessageDialogButtonResult.Ok;
            OnRequestDialogClose(true);
        }

        public ICommand ReplyCommand => new RelayCommand(execute => Reply());
        private void Reply()
        {
            InputDialogResult result = InputDialog.Show(LocalizedStrings.Instance["Input.Content.SendMessage"], LocalizedStrings.Instance["Input.Title.SendMessage"]);
            if (result.ButtonResult == InputDialogButtonResult.Ok && !string.IsNullOrEmpty(result.InputResult))
            {
                ReplyResult = result.InputResult;
                ButtonResult = MessageDialogButtonResult.Reply;
                OnRequestDialogClose(true);
            }
        }

        public ICommand RetryCommand => new RelayCommand(execute => Retry());
        private void Retry()
        {
            ButtonResult = MessageDialogButtonResult.Retry;
            OnRequestDialogClose(true);
        }

        public ICommand CancelCommand => new RelayCommand(execute => Cancel());
        private void Cancel()
        {
            ButtonResult = MessageDialogButtonResult.Cancel;
            OnRequestDialogClose(true);
        }

        public ICommand YesCommand => new RelayCommand(execute => Yes());
        private void Yes()
        {
            ButtonResult = MessageDialogButtonResult.Yes;
            OnRequestDialogClose(true);
        }

        public ICommand NoCommand => new RelayCommand(execute => No());
        private void No()
        {
            ButtonResult = MessageDialogButtonResult.No;
            OnRequestDialogClose(true);
        }
        #endregion
    }
}
