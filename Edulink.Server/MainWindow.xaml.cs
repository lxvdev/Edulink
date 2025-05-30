﻿using Edulink.Classes;
using Edulink.Communication.Models;
using Edulink.Core;
using Edulink.Models;
using Edulink.MVVM;
using Edulink.ViewModels;
using Edulink.Views;
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
            else if (command == Commands.Disconnect.ToString())
            {
                e.Client.Helper.Dispose();
            }
            else
            {
                Debug.WriteLine($"Unknown command received from {e.Client.Name}: {e.Command.Command}");
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
        private void FullScreenItem_Click(object sender, RoutedEventArgs e)
        {
            ToggleFullScreen();
        }

        private void AlwaysOnTopItem_CheckChanged(object sender, RoutedEventArgs e)
        {
            Topmost = (sender as MenuItem).IsChecked;
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

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                ToggleFullScreen();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MessageDialogResult dialogResult = MessageDialog.ShowLocalized("Main.AreYouSureYouWantToExit", MessageDialogTitle.Warning, MessageDialogButton.YesNo, MessageDialogIcon.Warning);
            if (dialogResult.ButtonResult == MessageDialogButtonResult.No)
            {
                e.Cancel = true;
                return;
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