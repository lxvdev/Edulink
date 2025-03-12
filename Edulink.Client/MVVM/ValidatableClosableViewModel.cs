using System;

namespace Edulink.MVVM
{
    public class ValidatableClosableViewModel : ValidatableViewModel
    {
        public event EventHandler RequestClose;
        protected virtual void OnRequestClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<bool> RequestDialogClose;
        protected virtual void OnRequestDialogClose(bool dialogResult)
        {
            RequestDialogClose?.Invoke(this, dialogResult);
        }
    }
}
