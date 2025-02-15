using Edulink.Server.MVVM;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Media;

namespace Edulink.Server.ViewModels
{
    public class MessageDialogViewModel
    {
        private Models.MessageDialog _messageDialog = new Models.MessageDialog();
        public Models.MessageDialog MessageDialog => _messageDialog;

        public MessageDialogViewModel(string message, string title, MessageDialogButton button, MessageDialogIcon icon)
        {
            _messageDialog.Message = message;
            _messageDialog.Title = title;
            SetMessageIcon(icon);
            SetButtonVisibility(button);
        }

        # region Icon and Button Setup
        public void SetMessageIcon(MessageDialogIcon icon)
        {
            switch (icon)
            {
                case MessageDialogIcon.Error:
                    _messageDialog.IconKind = PackIconKind.Error;
                    _messageDialog.IconColor = new SolidColorBrush(Colors.Red);
                    _messageDialog.IconVisibility = Visibility.Visible;
                    break;
                case MessageDialogIcon.Information:
                    _messageDialog.IconKind = PackIconKind.Info;
                    _messageDialog.IconColor = new SolidColorBrush(Colors.RoyalBlue);
                    _messageDialog.IconVisibility = Visibility.Visible;
                    break;
                case MessageDialogIcon.Warning:
                    _messageDialog.IconKind = PackIconKind.Warning;
                    _messageDialog.IconColor = new SolidColorBrush(Colors.Orange);
                    _messageDialog.IconVisibility = Visibility.Visible;
                    break;
                case MessageDialogIcon.Success:
                    _messageDialog.IconKind = PackIconKind.CheckCircle;
                    _messageDialog.IconColor = new SolidColorBrush(Colors.Green);
                    _messageDialog.IconVisibility = Visibility.Visible;
                    break;
                default:
                    _messageDialog.IconVisibility = Visibility.Collapsed;
                    break;
            }
        }

        public void SetButtonVisibility(MessageDialogButton button)
        {
            switch (button)
            {
                case MessageDialogButton.Ok:
                    _messageDialog.OkButtonVisibility = Visibility.Visible;
                    break;
                case MessageDialogButton.Reply:
                    _messageDialog.ReplyButtonVisibility = Visibility.Visible;
                    break;
                case MessageDialogButton.Cancel:
                    _messageDialog.CancelButtonVisibility = Visibility.Visible;
                    break;
                case MessageDialogButton.YesNo:
                    _messageDialog.YesButtonVisibility = Visibility.Visible;
                    _messageDialog.NoButtonVisibility = Visibility.Visible;
                    break;
                case MessageDialogButton.OkCancel:
                    _messageDialog.OkButtonVisibility = Visibility.Visible;
                    _messageDialog.CancelButtonVisibility = Visibility.Visible;
                    break;
                case MessageDialogButton.OkReply:
                    _messageDialog.OkButtonVisibility = Visibility.Visible;
                    _messageDialog.ReplyButtonVisibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Commands
        public RelayCommand OkCommand => new RelayCommand(execute => Ok());
        private void Ok()
        {
            _messageDialog.ButtonResult = MessageDialogButtonResult.Ok;
            OnRequestClose(true);
        }

        public RelayCommand ReplyCommand => new RelayCommand(execute => Reply());
        private void Reply()
        {
            InputDialogResult result = InputDialog.Show((string)Application.Current.TryFindResource("Input.Content.SendMessage"),
                                                        (string)Application.Current.TryFindResource("Input.Title.SendMessage"));
            if (result.ButtonResult == InputDialogButtonResult.Ok && !string.IsNullOrEmpty(result.ReplyResult))
            {
                _messageDialog.ReplyResult = result.ReplyResult;
                _messageDialog.ButtonResult = MessageDialogButtonResult.Reply;
                OnRequestClose(true);
            }
        }

        public RelayCommand CancelCommand => new RelayCommand(execute => Cancel());
        private void Cancel()
        {
            _messageDialog.ButtonResult = MessageDialogButtonResult.Cancel;
            OnRequestClose(true);
        }

        public RelayCommand YesCommand => new RelayCommand(execute => Yes());
        private void Yes()
        {
            _messageDialog.ButtonResult = MessageDialogButtonResult.Yes;
            OnRequestClose(true);
        }

        public RelayCommand NoCommand => new RelayCommand(execute => No());
        private void No()
        {
            _messageDialog.ButtonResult = MessageDialogButtonResult.No;
            OnRequestClose(true);
        }
        #endregion

        public event EventHandler<bool> RequestClose;
        protected virtual void OnRequestClose(bool dialogResult)
        {
            RequestClose?.Invoke(this, dialogResult);
        }
    }
}
