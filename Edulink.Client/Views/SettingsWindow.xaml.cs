using Edulink.ViewModels;
using System.Windows;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            SettingsWindowViewModel viewModel = new SettingsWindowViewModel();
            DataContext = viewModel;
            NameTextBox.Focus();
        }

        //private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    App.SetLanguage((LanguageComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString());
        //}

        //private void RefreshServiceStatus_Click(object sender, RoutedEventArgs e)
        //{
        //    UpdateServiceStatus();
        //}

        //private void UpdateServiceStatus()
        //{
        //    using (ServiceController serviceController = new ServiceController("EdulinkService"))
        //    {
        //        switch (serviceController.Status)
        //        {
        //            case ServiceControllerStatus.Running:
        //                UpdaterServiceStatus.SetResourceReference(Run.TextProperty, "Settings.Service.ServiceStatus.Running");
        //                break;
        //            case ServiceControllerStatus.Stopped:
        //                UpdaterServiceStatus.SetResourceReference(Run.TextProperty, "Settings.Service.ServiceStatus.Stopped");
        //                break;
        //            case ServiceControllerStatus.Paused:
        //                UpdaterServiceStatus.SetResourceReference(Run.TextProperty, "Settings.Service.ServiceStatus.Paused");
        //                break;
        //            case ServiceControllerStatus.StartPending:
        //                UpdaterServiceStatus.SetResourceReference(Run.TextProperty, "Settings.Service.ServiceStatus.Starting");
        //                break;
        //            case ServiceControllerStatus.StopPending:
        //                UpdaterServiceStatus.SetResourceReference(Run.TextProperty, "Settings.Service.ServiceStatus.Stopping");
        //                break;
        //        }
        //    }
        //}

        //private void StartUpdaterService_Click(object sender, RoutedEventArgs e)
        //{
        //    using (ServiceController serviceController = new ServiceController("EdulinkService"))
        //    {
        //        serviceController.Start();
        //    }
        //}
    }
}
