using System.Windows;
using System.Windows.Controls;

namespace Edulink.Attached
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty ListenToLengthProperty = DependencyProperty.RegisterAttached(
            "ListenToLength",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(false, OnListenToLengthChanged));

        public static readonly DependencyProperty InputLengthProperty = DependencyProperty.RegisterAttached(
            "InputLength",
            typeof(int),
            typeof(PasswordBoxHelper));

        public static bool GetListenToLength(DependencyObject obj) => (bool)obj.GetValue(ListenToLengthProperty);
        public static void SetListenToLength(DependencyObject obj, bool value) => obj.SetValue(ListenToLengthProperty, value);

        public static int GetInputLength(DependencyObject obj) => (int)obj.GetValue(InputLengthProperty);
        public static void SetInputLength(DependencyObject obj, int value) => obj.SetValue(InputLengthProperty, value);

        private static void OnListenToLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                }
                else
                {
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                }
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetInputLength(passwordBox, passwordBox.Password.Length);
            }
        }
    }

}
