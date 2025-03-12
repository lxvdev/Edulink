using Edulink.MVVM;
using Edulink.Views;
using System.Windows.Input;

namespace Edulink.ViewModels
{
    internal class InputDialogViewModel : ClosableViewModel
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
        public ICommand OkCommand => new RelayCommand(execute => Ok(), canExecute => !string.IsNullOrEmpty(InputValue));
        private void Ok()
        {
            ButtonResult = InputDialogButtonResult.Ok;
            OnRequestDialogClose(true);
        }

        public ICommand CancelCommand => new RelayCommand(execute => Cancel());
        private void Cancel()
        {
            ButtonResult = InputDialogButtonResult.Cancel;
            OnRequestDialogClose(true);
        }
        #endregion
    }
}