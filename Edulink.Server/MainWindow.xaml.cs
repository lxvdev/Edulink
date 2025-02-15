using Edulink.Server.Communication.Models;
using Edulink.Server.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Edulink.Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SettingsWindow _settingsWindow;
        private AboutDialog _aboutDialog;
        private Core.Server _server;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                _server = new Core.Server(App.SettingsManager.Settings.Port);
                _server.CommandReceived += Server_CommandReceived;

                MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(_server);
                DataContext = mainWindowViewModel;
                Loaded += async (s, e) => await _server.StartServerAsync();
            }
            catch (Exception)
            {
                MessageDialog.Show((string)Application.Current.TryFindResource("Message.Content.CouldntInitializeServer"),
                    MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);

                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Owner = null;
                settingsWindow.Show();

                Close();
            }
        }

        #region Handle received commands
        private void Server_CommandReceived(object sender, Core.Server.CommandReceivedEventArgs e)
        {
            switch (e.Command.Command)
            {
                case "DESKTOP":
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        HandleDesktop(e);
                    });
                    break;
                case "PREVIEW":
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        HandlePreview(e);
                    });
                    break;
                case "MESSAGE":
                    _ = Application.Current.Dispatcher.Invoke(async () =>
                    {
                        await HandleMessageAsync(e);
                    });
                    break;
                default:
                    Console.WriteLine($"Unknown command received from {e.Client.Name}: {e.Command.Command}");
                    break;
            }
        }

        private void HandleDesktop(Core.Server.CommandReceivedEventArgs e)
        {
            if (e.Command.Content != null)
            {
                DesktopPreviewDialog existingDialog = ((MainWindowViewModel)DataContext).openDesktopPreviewDialogs.FirstOrDefault(dialog => dialog.Client == e.Client);
                if (existingDialog == null)
                {
                    existingDialog = new DesktopPreviewDialog(e.Client);
                    ((MainWindowViewModel)DataContext).openDesktopPreviewDialogs.Add(existingDialog);
                    existingDialog.Closed += (s, _) => ((MainWindowViewModel)DataContext).openDesktopPreviewDialogs.Remove(existingDialog);
                    existingDialog.Show();
                }

                using (MemoryStream ms = new MemoryStream(e.Command.Content))
                {
                    Bitmap image = new Bitmap(ms);
                    existingDialog.UpdateDesktop(image);
                }
            }
        }

        private void HandlePreview(Core.Server.CommandReceivedEventArgs e)
        {
            if (e.Command.Content != null)
            {
                BitmapImage bitmapImage = new BitmapImage();
                using (MemoryStream ms = new MemoryStream(e.Command.Content))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }
                bitmapImage.Freeze();
                e.Client.DesktopPreview = bitmapImage;
            }
        }

        private async Task HandleMessageAsync(Core.Server.CommandReceivedEventArgs e)
        {
            MessageDialogResult messageDialogResult = MessageDialog.Show(e.Command.Parameters["Message"], string.Format((string)Application.Current.TryFindResource("Message.Title.MessageFrom"), e.Client.Name),
                MessageDialogButton.OkReply);
            if (messageDialogResult.ButtonResult == MessageDialogButtonResult.Reply && !string.IsNullOrEmpty(messageDialogResult.ReplyResult))
            {
                await e.Client.Helper.SendCommandAsync(new EdulinkCommand
                {
                    Command = e.Command.Command,
                    Parameters = new Dictionary<string, string>
                        {
                            { "Message", messageDialogResult.ReplyResult }
                        }
                });
            }
        }

        #endregion

        #region Menu Items
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsWindow == null || !_settingsWindow.IsVisible)
            {
                _settingsWindow = new SettingsWindow();
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.Focus();
            }
        }

        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            if (_aboutDialog == null || !_aboutDialog.IsVisible)
            {
                _aboutDialog = new AboutDialog();
                _aboutDialog.Show();
            }
            else
            {
                _aboutDialog.Focus();
            }
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            App.CloseApp();
        }
        #endregion

        private void ComputersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                if (!viewModel.ViewDesktopCommand.CanExecute(null))
                    return;
                viewModel.ViewDesktopCommand.Execute(null);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _settingsWindow = null;
            _server?.Dispose();
        }
    }
}
