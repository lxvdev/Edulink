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
            //SettingsViewModel settingsViewModel = new SettingsViewModel();
            //DataContext = settingsViewModel;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.SetLanguage((LanguageComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString());
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            App.CloseApp();
        }
    }
}
