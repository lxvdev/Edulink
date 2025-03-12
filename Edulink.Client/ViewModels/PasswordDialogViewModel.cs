using Edulink.Classes;
using Edulink.MVVM;
using Edulink.Views;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using WPFLocalizeExtension.Engine;

namespace Edulink.ViewModels
{
    public class PasswordDialogViewModel : ClosableViewModel
    {
        public string Title
        {
            get
            {
                if (_dialogType == PasswordDialogType.EnterPassword)
                {
                    return "PasswordDialog.Title.EnterPassword";
                }
                else if (_dialogType == PasswordDialogType.SetOrChangePassword)
                {
                    if (_isChangingPassword)
                    {
                        return "PasswordDialog.Title.ChangePassword";
                    }
                    return "PasswordDialog.Title.SetPassword";
                }
                return string.Empty;
            }
        }

        // Password fields
        private string _currentPassword;
        public string CurrentPassword
        {
            get => _currentPassword;
            set
            {
                if (_currentPassword != value)
                {
                    _currentPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                    CalculatePasswordStrength();
                }
            }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (_confirmPassword != value)
                {
                    _confirmPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        // Loading
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        // Password strength properties
        private int _passwordStrength = 0;
        public int PasswordStrength
        {
            get => _passwordStrength;
            private set
            {
                if (_passwordStrength != value)
                {
                    _passwordStrength = value;
                    OnPropertyChanged();
                }
            }
        }

        public SolidColorBrush StrengthColor
        {
            get
            {
                SolidColorBrush strengthColor;
                if (PasswordStrength >= 80)
                {
                    strengthColor = new SolidColorBrush(Colors.Green);
                }
                else if (PasswordStrength >= 60)
                {
                    strengthColor = new SolidColorBrush(Colors.YellowGreen);
                }
                else if (PasswordStrength >= 40)
                {
                    strengthColor = new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    strengthColor = new SolidColorBrush(Colors.Red);
                }
                return strengthColor;
            }
        }

        public string StrengthText
        {
            get
            {
                string strengthText;
                if (PasswordStrength >= 80)
                {
                    strengthText = "PasswordDialog.Strength.VeryStrong";
                }
                else if (PasswordStrength >= 60)
                {
                    strengthText = "PasswordDialog.Strength.Strong";
                }
                else if (PasswordStrength >= 40)
                {
                    strengthText = "PasswordDialog.Strength.Medium";
                }
                else
                {
                    strengthText = "PasswordDialog.Strength.Weak";
                }
                return strengthText;
            }
        }

        // Dialog type
        private bool _isChangingPassword;
        public bool IsChangingPassword
        {
            get => _isChangingPassword;
            set
            {
                if (_isChangingPassword != value)
                {
                    _isChangingPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly PasswordDialogType _dialogType;

        public bool IsSetOrChangePassword => _dialogType == PasswordDialogType.SetOrChangePassword;

        public PasswordDialogViewModel(PasswordDialogType type)
        {
            _dialogType = string.IsNullOrEmpty(App.SettingsManager.Settings.Password) ? PasswordDialogType.SetOrChangePassword : type;
            _isChangingPassword = !string.IsNullOrEmpty(App.SettingsManager.Settings.Password);

            LocalizeDictionary.Instance.PropertyChanged += LocalizeDictionary_PropertyChanged;
        }

        private void LocalizeDictionary_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalizeDictionary.Instance.Culture))
            {
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(StrengthText));
            }
        }

        #region Commands
        public ICommand ConfirmCommand => new RelayCommand(async execute => await ConfirmAsync(), canExecute => !IsLoading);
        private async Task ConfirmAsync()
        {
            if (await HandlePasswordAsync())
            {
                OnRequestDialogClose(true);
            }
        }

        public ICommand CancelCommand => new RelayCommand(execute => Cancel());
        private void Cancel()
        {
            OnRequestDialogClose(false);
        }
        #endregion

        private async Task<bool> HandlePasswordAsync()
        {
            try
            {
                IsLoading = true;
                if (_dialogType == PasswordDialogType.EnterPassword)
                {
                    // Check if the password field is empty
                    if (string.IsNullOrEmpty(_currentPassword))
                        throw new InvalidOperationException("Message.Content.EnterAPassword");

                    // Check if the password is correct
                    bool isValid = await Task.Run(() => HashUtility.VerifyPassword(_currentPassword, App.SettingsManager.Settings.Password));

                    if (!isValid)
                        throw new InvalidOperationException("Message.Content.IncorrectPassword");
                }
                else if (_dialogType == PasswordDialogType.SetOrChangePassword)
                {
                    // Check if the password fields are empty
                    if (_isChangingPassword && string.IsNullOrEmpty(_currentPassword))
                        throw new InvalidOperationException("Message.Content.EnterAPassword");

                    if (_isChangingPassword && !HashUtility.VerifyPassword(_currentPassword, App.SettingsManager.Settings.Password))
                        throw new InvalidOperationException("Message.Content.IncorrectPassword");

                    if (_password != _confirmPassword)
                        throw new InvalidOperationException("Message.Content.PasswordsDoNotMatch");

                    // Check if the password is strong enough
                    if (_passwordStrength <= 40)
                    {
                        MessageDialogResult userChoice = MessageDialog.ShowLocalized("Message.Content.AreYouSureWeakPassword", MessageDialogTitle.Warning, MessageDialogButton.YesNo, MessageDialogIcon.Warning);

                        if (userChoice.ButtonResult == MessageDialogButtonResult.No)
                            return false;
                    }

                    // Set the new password
                    await Task.Run(() => App.SettingsManager.Settings.Password = string.IsNullOrEmpty(_password) ? null : HashUtility.HashPassword(_password));
                    App.SettingsManager.Save();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageDialog.ShowLocalized(ex.Message, MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CalculatePasswordStrength()
        {
            int score = Password.Length * 10;
            if (Password.Length >= 8 && Password.Any(char.IsDigit)) score += 20;
            if (Password.Any(char.IsUpper) && Password.Any(char.IsLower)) score += 20;
            if (Password.Any(ch => "!@#$%^&*()_+".Contains(ch))) score += 20;
            PasswordStrength = Math.Min(score, 100);

            OnPropertyChanged(nameof(StrengthColor));
            OnPropertyChanged(nameof(StrengthText));
        }
    }
}
