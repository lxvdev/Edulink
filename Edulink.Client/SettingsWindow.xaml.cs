using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace Edulink.Client
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            NameTextBox.Text = App.configManager.Settings?.Name;
            IPAddressTextBox.Text = App.configManager.Settings?.IPAddress;
            PortTextBox.Text = App.configManager.Settings?.Port.ToString();
            NameTextBox.Focus();

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

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            App.CloseApp();
        }

        private void ReloadConnection_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            App.RestartApp();
        }

        private void SaveSettings()
        {
            try
            {
                if (string.IsNullOrEmpty(NameTextBox.Text))
                {
                    throw new Exception("Name cannot be empty.");
                }

                if (string.IsNullOrEmpty(IPAddressTextBox.Text))
                {
                    throw new Exception("IP Address cannot be empty.");
                }

                if (!IPAddress.TryParse(IPAddressTextBox.Text, out _))
                {
                    throw new Exception("Invalid IP Address.");
                }

                if (string.IsNullOrEmpty(PortTextBox.Text))
                {
                    throw new Exception("Port cannot be empty.");
                }

                if (!int.TryParse(PortTextBox.Text, out int port))
                {
                    throw new Exception("Port must be a valid number.");
                }

                App.configManager.Settings.Name = NameTextBox.Text;
                App.configManager.Settings.IPAddress = IPAddressTextBox.Text;
                App.configManager.Settings.Port = port;
                App.configManager.Settings.Language = (LanguageComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

                App.configManager.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving settings: {ex.Message}");
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.SetLanguageDictionary((LanguageComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString());
        }
    }
}
