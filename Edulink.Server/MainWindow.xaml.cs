using Edulink.Classes;
using Edulink.Communication.Models;
using Edulink.Core;
using Edulink.Models;
using Edulink.MVVM;
using Edulink.ViewModels;
using Edulink.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SettingsWindow _settingsWindow;
        private AboutDialog _aboutDialog;
        private Server _server;

        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTrayIcon();
            try
            {
                _server = new Server(App.SettingsManager.Settings.Port);
                _server.CommandReceived += Server_CommandReceivedAsync;

                _viewModel = new MainWindowViewModel(_server);
                DataContext = _viewModel;
                Loaded += async (s, e) => await _server.StartServerAsync();
            }
            catch (Exception)
            {
                MessageDialog.ShowLocalized("Message.Content.CouldntInitializeServer", MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);

                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Show();

                Close();
            }
        }

        private void InitializeTrayIcon()
        {
            RelayCommand showWindowCommand = new RelayCommand(execute =>
            {
                this.WindowState = WindowState.Normal;
                this.Activate();
            });

            App.TaskbarIcon.LeftClickCommand = showWindowCommand;
            App.TaskbarIcon.DoubleClickCommand = showWindowCommand;

            App.TaskbarIcon.TrayBalloonTipClicked += TaskbarIcon_TrayBalloonTipClicked;
        }

        private void TaskbarIcon_TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            if (App.ActiveBalloonTipType == App.BalloonTipType.ComputerDisconnected)
            {
                this.WindowState = WindowState.Normal;
                this.Activate();
            }
        }

        #region Handle received commands
        private async void Server_CommandReceivedAsync(object sender, Server.CommandReceivedEventArgs e)
        {
            switch (e.Command.Command)
            {
                case Commands.Desktop:
                    HandleDesktop(e);
                    break;
                case Commands.Preview:
                    HandlePreview(e);
                    break;
                case Commands.Message:
                    await HandleMessageAsync(e);
                    break;
                case Commands.Disconnect:
                    e.Client.Helper.Dispose();
                    break;
                default:
                    Console.WriteLine($"Unknown command received from {e.Client.Name}: {e.Command.Command}");
                    break;
            }
        }

        private void HandleDesktop(Server.CommandReceivedEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (e.Command.Content != null)
            {
                DesktopDialog existingDialog = _viewModel.openDesktopDialogs.FirstOrDefault(dialog => dialog.Client == e.Client);
                if (existingDialog == null)
                {
                    existingDialog = new DesktopDialog(e.Client);
                    _viewModel.openDesktopDialogs.Add(existingDialog);
                    existingDialog.Closed += (s, _) => _viewModel.openDesktopDialogs.Remove(existingDialog);
                    existingDialog.Show();
                }

                using (MemoryStream ms = new MemoryStream(e.Command.Content))
                {
                    Bitmap image = new Bitmap(ms);
                    existingDialog.UpdateDesktop(image);
                }
            }
        }

        private void HandlePreview(Server.CommandReceivedEventArgs e)
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
                e.Client.Preview = bitmapImage;
            }
        }

        private async Task HandleMessageAsync(Server.CommandReceivedEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            MessageDialogResult messageDialogResult = MessageDialog.Show(e.Command.Parameters["Message"], string.Format(LocalizedStrings.Instance["Message.Title.MessageFrom"], e.Client.Name), MessageDialogButton.OkReply);
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
        private void SettingsItem_Click(object sender, RoutedEventArgs e)
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
                if (!viewModel.DesktopCommand.CanExecute(null))
                    return;
                viewModel.DesktopCommand.Execute(null);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (App.TaskbarIcon != null)
            {
                App.TaskbarIcon.LeftClickCommand = null;
                App.TaskbarIcon.DoubleClickCommand = null;
                App.TaskbarIcon.Dispose();
            }
            _settingsWindow = null;
            _server?.Dispose();
        }

        private void AlwaysOnTopItem_CheckChanged(object sender, RoutedEventArgs e)
        {
            this.Topmost = (sender as MenuItem).IsChecked;
        }

        private void UpdaterItem_Click(object sender, RoutedEventArgs e)
        {
            UpdaterDialog updaterDialog = new UpdaterDialog();
            updaterDialog.Show();
        }
    }
}
