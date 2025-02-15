using Edulink.Server.MVVM;
using System;

namespace Edulink.Server.ViewModels
{
    internal class InputDialogViewModel : ViewModelBase
    {
        public string Message { get; private set; }
        public string Title { get; private set; }

        public InputDialogButtonResult ButtonResult { get; private set; }

        private string _inputValue;
        public string InputValue
        {
            get => _inputValue;
            set
            {
                if (_inputValue != value)
                {
                    _inputValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public InputDialogViewModel(string message, string title)
        {
            Message = message;
            Title = title;
        }

        #region Commands    
        public RelayCommand OkCommand => new RelayCommand(execute => Ok(), canExecute => !string.IsNullOrEmpty(InputValue));
        private void Ok()
        {
            ButtonResult = InputDialogButtonResult.Ok;
            OnRequestClose(true);
        }

        public RelayCommand CancelCommand => new RelayCommand(execute => Cancel());
        private void Cancel()
        {
            ButtonResult = InputDialogButtonResult.Cancel;
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