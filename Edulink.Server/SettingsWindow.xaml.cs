using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            PortTextBox.Text = App.configManager.Settings?.Port.ToString();
            PortTextBox.Focus();

            LanguageComboBox.SelectedItem = LanguageComboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Tag != null && item.Tag.Equals(App.configManager.Settings?.Language));
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            App.RestartApp();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            App.configManager.Reset();
            App.RestartApp();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(PortTextBox.Text))
                {
                    throw new Exception("Port cannot be empty.");
                }
                if (PortTextBox.Text.Length > 4)
                {
                    throw new Exception("Port cannot be larger than 4 numbers.");
                }

                if (!int.TryParse(PortTextBox.Text, out int port))
                {
                    throw new Exception("Port must be a valid number.");
                }
                App.configManager.Settings.Port = port;
                App.configManager.Settings.Language = (LanguageComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

                App.configManager.Save();

                App.RestartApp();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.SetLanguageDictionary((LanguageComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Collapsed;
        }
    }
}
