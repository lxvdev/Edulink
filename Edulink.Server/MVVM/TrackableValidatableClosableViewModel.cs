using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Edulink.MVVM
{
    public class TrackableValidatableClosableViewModel : ValidatableClosableViewModel
    {
        private readonly HashSet<string> _unsavedProperties = new HashSet<string>();

        public bool HasUnsavedChanges => _unsavedProperties.Count > 0;

        protected void TrackUnsavedChanges(object originalValue, [CallerMemberName] string propertyName = null)
        {
            object newValue = GetType().GetProperty(propertyName)?.GetValue(this);

            if (!Equals(originalValue, newValue))
            {
                _unsavedProperties.Add(propertyName);
            }
            else
            {
                _unsavedProperties.Remove(propertyName);
            }

            OnPropertyChanged(nameof(HasUnsavedChanges));
        }

        protected void ClearUnsavedChanges()
        {
            _unsavedProperties.Clear();
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }
    }
}
