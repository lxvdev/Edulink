using Edulink.Classes;
using Edulink.Communication.Models;
using Edulink.Core;
using Edulink.Models;
using Edulink.MVVM;
using Edulink.ViewModels;
using Edulink.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
                _viewModel.RequestClose += (_, e) => Close();
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

        private void ShowWindow()
        {
            WindowState = WindowState.Normal;
            Activate();
        }

        #region Tray icon
        private void InitializeTrayIcon()
        {
            App.TaskbarIcon.LeftClickCommand = new RelayCommand(execute => ShowWindow());
            App.TaskbarIcon.DoubleClickCommand = new RelayCommand(execute => ShowWindow());

            App.TaskbarIcon.TrayBalloonTipClicked += TaskbarIcon_TrayBalloonTipClicked;
        }

        private void TaskbarIcon_TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            if (App.ActiveBalloonTipType == BalloonTipType.ComputerDisconnected)
            {
                ShowWindow();
            }
        }
        #endregion

        private void ToggleFullScreen()
        {
            if (WindowStyle == WindowStyle.None)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                ToggleFullScreen();
            }
        }

        #region Handle received commands
        private async void Server_CommandReceivedAsync(object sender, Server.CommandReceivedEventArgs e)
        {
            string command = e.Command.Command;
            if (command == Commands.ViewDesktop.ToString())
            {
                HandleDesktop(e);
            }
            else if (command == Commands.Preview.ToString())
            {
                HandlePreview(e);
            }
            else if (command == Commands.Message.ToString())
            {
                await HandleMessageAsync(e);
            }
            else if (command == Commands.ComputerList.ToString())
            {
                await HandleComputerList(e);
            }
            else if (command == Commands.Disconnect.ToString())
            {
                e.Client.Helper.Dispose();
            }
            else
            {
                Debug.WriteLine($"Unknown command received from {e.Client.Name}: {e.Command.Command}");
            }
        }

        #region Handle computer list
        private async Task HandleComputerList(Server.CommandReceivedEventArgs e)
        {
            // File sharing settings
            bool isFileSharingBetweenStudentsAllowed = App.SettingsManager.Settings.FileSharingStudents;
            bool isFileSharingToTeacherAllowed = App.SettingsManager.Settings.FileSharingTeacher;

            if (!isFileSharingBetweenStudentsAllowed)
            {
                await SendComputerListResponseAsync(e.Client, false, isFileSharingToTeacherAllowed);
                return;
            }

            // Convert Clients to Computers
            List<Computer> computers = _viewModel.Clients
                /*.Where(client => client.ID != e.Client.ID) */// Remove sender computer
                .Select(client => new Computer
                {
                    Name = client.Name,
                    ID = client.ID
                }).ToList();

            // Serialize
            string computersJson = JsonConvert.SerializeObject(computers, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            // Send the response
            await SendComputerListResponseAsync(e.Client, true, isFileSharingToTeacherAllowed, computersJson);
        }

        private async Task SendComputerListResponseAsync(Client client, bool success, bool sendTeacher, string list = null)
        {
            // Basic parameters
            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "Success", success.ToString() },
                { "SendTeacher", sendTeacher.ToString() },
                { "ClientID", client.ID.ToString() }
            };

            // Add list to parameters if it has been provided on the parameters of this function
            if (list != null)
            {
                parameters.Add("List", list);
            }

            // Send response to the client
            await client.Helper.SendCommandAsync(new EdulinkCommand
            {
                Command = Commands.ComputerList.ToString(),
                Parameters = parameters
            });
        }
        #endregion

        private void HandleDesktop(Server.CommandReceivedEventArgs e)
        {
            if (e is null) throw new ArgumentNullException(nameof(e));

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

        // Set the client preview to the received image preview
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
            if (e is null) throw new ArgumentNullException(nameof(e));

            MessageDialogResult messageDialogResult = MessageDialog.Show(e.Command.Parameters["Message"],
                                                                         string.Format(LocalizedStrings.Instance["Message.Title.MessageFrom"], e.Client.Name),
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
        private void FullScreenItem_Click(object sender, RoutedEventArgs e)
        {
            ToggleFullScreen();
        }

        private void AlwaysOnTopItem_CheckChanged(object sender, RoutedEventArgs e)
        {
            Topmost = (sender as MenuItem).IsChecked;
        }
        #endregion

        // Show desktop dialog whenever a computer has been clicked twice on the computer list
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
            // Ask user if they really want to exit the application
            MessageDialogResult dialogResult = MessageDialog.ShowLocalized("Main.AreYouSureYouWantToExit", MessageDialogTitle.Warning, MessageDialogButton.YesNo, MessageDialogIcon.Warning);
            if (dialogResult.ButtonResult == MessageDialogButtonResult.No)
            {
                e.Cancel = true;
                return; // Fun fact: I forgot to add this so it would dispose it even if you said no
            }

            if (App.TaskbarIcon != null)
            {
                App.TaskbarIcon.LeftClickCommand = null;
                App.TaskbarIcon.DoubleClickCommand = null;
                App.TaskbarIcon.Dispose();
            }

            _server?.Dispose();
        }
    }
}