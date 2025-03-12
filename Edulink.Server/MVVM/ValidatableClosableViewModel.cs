using System;

namespace Edulink.MVVM
{
    public class ValidatableClosableViewModel : ValidatableViewModel
    {
        protected event EventHandler RequestClose;
        protected virtual void OnRequestClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        protected event EventHandler<bool> RequestDialogClose;
        protected virtual void OnRequestDialogClose(bool dialogResult)
        {
            RequestDialogClose?.Invoke(this, dialogResult);
        }
    }
}
