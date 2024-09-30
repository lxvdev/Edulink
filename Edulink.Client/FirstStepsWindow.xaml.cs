using Edulink.Client;
using System;
using System.Net;
using System.Windows;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for FirstStepsWindow.xaml
    /// </summary>
    public partial class FirstStepsWindow : Window
    {
        public FirstStepsWindow()
        {
            InitializeComponent();
            NameTextBox.Text = App.configManager.Settings?.Name;
            IPAddressTextBox.Text = App.configManager.Settings?.IPAddress;
            PortTextBox.Text = App.configManager.Settings?.Port.ToString();
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
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

                App.configManager.Save();

                Close();

                //App.RestartApp();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving settings: {ex.Message}");
            }
        }
    }
}
