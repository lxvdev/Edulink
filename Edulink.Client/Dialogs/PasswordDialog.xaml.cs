using Edulink.Client;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Edulink.Classes
{
    /// <summary>
    /// Interaction logic for PasswordDialog.xaml
    /// </summary>
    public partial class PasswordDialog : Window
    {
        private string _password
        {
            get
            {
                return App.SettingsManager.Settings.Password;
            }
            set
            {
                App.SettingsManager.Settings.Password = value;
                App.SettingsManager.Save();
            }
        }

        private readonly bool _isChangingPassword;
        private readonly PasswordDialogType _dialogType;

        public PasswordDialog(PasswordDialogType type)
        {
            _dialogType = string.IsNullOrEmpty(_password) ? PasswordDialogType.SetOrChangePassword : type;
            _isChangingPassword = !string.IsNullOrEmpty(_password);

            InitializeComponent();
            InitializeUI(_dialogType);
        }

        private void InitializeUI(PasswordDialogType type)
        {
            switch (type)
            {
                case PasswordDialogType.EnterPassword:
                    PasswordTitle.SetResourceReference(TextBlock.TextProperty, "Password.Title.EnterPassword");
                    CurrentPasswordBox.Visibility = Visibility.Collapsed;
                    ConfirmPasswordBox.Visibility = Visibility.Collapsed;
                    StrengthStatus.Visibility = Visibility.Collapsed;
                    StrengthBar.Visibility = Visibility.Collapsed;
                    PasswordBox.Focus();
                    break;
                case PasswordDialogType.SetOrChangePassword:
                    PasswordTitle.SetResourceReference(TextBlock.TextProperty, _isChangingPassword ? "Password.Title.ChangePassword" : "Password.Title.SetPassword");
                    CurrentPasswordBox.Visibility = _isChangingPassword ? Visibility.Visible : Visibility.Collapsed;
                    ConfirmPasswordBox.Visibility = Visibility.Visible;
                    StrengthStatus.Visibility = Visibility.Visible;
                    StrengthBar.Visibility = Visibility.Visible;
                    (_isChangingPassword ? CurrentPasswordBox : PasswordBox).Focus();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Invalid dialog type.");
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HandlePassword();
        }

        private void HandlePassword()
        {
            try
            {
                string inputPassword = PasswordBox.Password;

                switch (_dialogType)
                {
                    case PasswordDialogType.EnterPassword:
                        if (!string.IsNullOrEmpty(inputPassword) && !HashUtility.VerifyPassword(inputPassword, _password))
                        {
                            throw new InvalidOperationException((string)Application.Current.TryFindResource("Message.Content.IncorrectPassword"));
                        }
                        else if (string.IsNullOrEmpty(inputPassword))
                        {
                            throw new InvalidOperationException((string)Application.Current.TryFindResource("Message.Content.EnterAPassword"));

                        }

                        DialogResult = true;
                        Close();
                        break;

                    case PasswordDialogType.SetOrChangePassword:
                        HandleSetOrChangePassword(inputPassword);
                        break;

                    default:
                        throw new InvalidOperationException("Unhandled password dialog type.");
                }
            }
            catch (Exception ex)
            {
                MessageDialog.Show(ex.Message.ToString(), null, MessageDialogButtons.Ok, MessageDialogType.Error);
            }
        }

        private void HandleSetOrChangePassword(string newPassword)
        {
            string confirmPassword = ConfirmPasswordBox.Password;

            if (_isChangingPassword)
            {
                string currentPassword = CurrentPasswordBox.Password;

                if (!HashUtility.VerifyPassword(currentPassword, _password))
                {
                    throw new InvalidOperationException((string)Application.Current.TryFindResource("Message.Content.IncorrectPassword"));
                }
            }

            if (newPassword != confirmPassword)
            {
                throw new InvalidOperationException((string)Application.Current.TryFindResource("Message.Content.PasswordsDoNotMatch"));
            }

            if (CalculatePasswordStrength(newPassword) <= 40)
            {
                MessageDialogResult userChoice = MessageDialog.Show(
                    (string)Application.Current.TryFindResource("Message.Content.WeakPassword"),
                    null,
                    MessageDialogButtons.YesNo,
                    MessageDialogType.Warning
                );

                if (userChoice != MessageDialogButtonResult.Yes)
                {
                    return;
                }
            }

            _password = HashUtility.HashPassword(newPassword);

            DialogResult = true;
            Close();
        }

        private int CalculatePasswordStrength(string password)
        {
            int score = password.Length * 10;
            if (password.Length >= 8 && password.Any(char.IsDigit)) score += 20;
            if (password.Any(char.IsUpper) && password.Any(char.IsLower)) score += 20;
            if (password.Any(ch => "!@#$%^&*()_+".Contains(ch))) score += 20;
            return Math.Min(score, 100);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            int score = CalculatePasswordStrength(PasswordBox.Password);
            StrengthBar.Value = score;

            string strengthKey;
            SolidColorBrush strengthColor;

            if (score >= 80)
            {
                strengthKey = "Password.Strength.VeryStrong";
                strengthColor = new SolidColorBrush(Colors.Green);
            }
            else if (score >= 60)
            {
                strengthKey = "Password.Strength.Strong";
                strengthColor = new SolidColorBrush(Colors.YellowGreen);
            }
            else if (score >= 40)
            {
                strengthKey = "Password.Strength.Medium";
                strengthColor = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                strengthKey = "Password.Strength.Weak";
                strengthColor = new SolidColorBrush(Colors.Red);
            }

            StrengthState.SetResourceReference(Run.TextProperty, strengthKey);
            StrengthBar.Foreground = strengthColor;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandlePassword();
            }
        }

        public static bool Show(PasswordDialogType type)
        {
            PasswordDialog dialog = new PasswordDialog(type);

            if (type == PasswordDialogType.EnterPassword && string.IsNullOrEmpty(dialog._password))
            {
                MessageDialogResult userChoice = MessageDialog.Show(
                    (string)Application.Current.TryFindResource("Message.Content.NoPasswordSet"),
                    null,
                    MessageDialogButtons.YesNo,
                    MessageDialogType.Warning
                );

                if (userChoice != MessageDialogButtonResult.Yes)
                {
                    return true;
                }
            }

            return dialog.ShowDialog() == true ? (bool)dialog.DialogResult : false;
        }
    }

    public enum PasswordDialogType
    {
        EnterPassword,
        SetOrChangePassword,
    }
}
