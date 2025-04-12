using Edulink.Communication;
using Edulink.Communication.Models;
using Edulink.Models;
using Edulink.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for SendFileWindow.xaml
    /// </summary>
    public partial class SendFileWindow : Window
    {
        private SendFileWindowViewModel _viewModel;

        private FileSharing _fileSharing;

        public SendFileWindow()
        {
            InitializeComponent();
            _viewModel = new SendFileWindowViewModel();
            _fileSharing = new FileSharing();
            DataContext = _viewModel;

            // Request computer list whenever the connection status changes
            App.Client.ConnectionStatusChanged += (_, status) =>
            {
                _viewModel.PageIndex = 0;
            };

            // Request sending file to the target computer
            _viewModel.RequestSendFile += async (_, e) => await RequestSendFile();

            _viewModel.RequestComputerList += async (_, e) => await RequestComputerList();

            // File sharing progress
            _fileSharing.ProgressChanged += (_, bytes) => _viewModel.Progress = (int)((double)bytes / _viewModel.File.Length * 100);

            _fileSharing.ConnectionChanged += (_, connected) =>
            {
                if (connected)
                {
                    _viewModel.IsSendingFile = true;
                }
            };
        }


        #region Receiving list
        private async Task RequestComputerList()
        {
            try
            {
                if (App.Client.Connected)
                {
                    _viewModel.IsReceivingList = true;
                    await App.Client.Helper?.SendCommandAsync(new EdulinkCommand() { Command = Commands.ComputerList.ToString() });
                }
                else
                {
                    _viewModel.IsReceivingList = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not request computer list: {ex.Message}");
                _viewModel.IsReceivingList = false;
            }
        }

        public void UpdateList(List<Computer> computerList)
        {
            _viewModel.UpdateList(computerList);
        }

        public void SetSharingStatus(bool student, bool teacher)
        {
            _viewModel.SetSharingStatus(student, teacher);
        }
        #endregion

        #region Selecting file
        private void SelectFileButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                SetFile(files.FirstOrDefault());
            }
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            SetFile(openFileDialog.FileName);
        }

        private void SetFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            try
            {
                FileInfo fileInfo = new FileInfo(path);

                if (!fileInfo.Exists)
                {
                    Debug.WriteLine("Invalid file path provided.");
                    MessageDialog.ShowLocalized("Message.Content.InvalidFilePath", MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
                    return;
                }

                if (fileInfo.Length > 200 * 1024 * 1024) // 200MB
                {
                    MessageDialog.ShowLocalized("Message.Content.TheFileCannotBeLargerThan200MB", MessageDialogTitle.Warning, MessageDialogButton.Ok, MessageDialogIcon.Warning);
                    return;
                }

                // Check if the file is accessible
                using (FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    _viewModel.SetFile(fileInfo);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"Unauthorized access: {ex.Message}");
                MessageDialog.ShowLocalized("Message.Content.UnauthorizedAccess", MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
            }
            catch (PathTooLongException ex)
            {
                Debug.WriteLine($"Path too long: {ex.Message}");
                MessageDialog.ShowLocalized("Message.Content.PathTooLong", MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"File I/O error: {ex.Message}");
                MessageDialog.ShowLocalized("Message.Content.FileInUse", MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error: {ex.Message}");
                MessageDialog.ShowLocalized("Message.Content.UnexpectedError", MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
            }
        }
        #endregion

        #region Sending file
        private CancellationTokenSource _responseTimeoutCts;

        public async Task RequestSendFile()
        {
            try
            {
                if (App.Client.Connected)
                {
                    _viewModel.IsReceivingResponse = true;
                    await App.Client.Helper?.SendCommandAsync(new EdulinkCommand()
                    {
                        Command = Commands.RequestSendFile.ToString(),
                        Parameters = new Dictionary<string, string>()
                        {
                            { "TargetComputer", JsonConvert.SerializeObject(_viewModel.TargetComputer) },
                            { "FileName", _viewModel.File.Name },
                            { "FileLength", _viewModel.File.Length.ToString() }
                        }
                    });

                    // Start response timeout
                    _responseTimeoutCts = new CancellationTokenSource();

                    await Task.Delay(30000, _responseTimeoutCts.Token);

                    _viewModel.IsReceivingResponse = false;

                    _viewModel.Reset();
                }
                else
                {
                    _viewModel.IsReceivingResponse = false;
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("Response received");
                _viewModel.IsReceivingResponse = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not request send file: {ex.Message}");
                _viewModel.IsReceivingResponse = false;
            }
        }

        public async void HandleResponse(bool accepted, IEnumerable<IPEndPoint> ipEndPoints = null)
        {
            _responseTimeoutCts?.Cancel();

            try
            {
                if (accepted && ipEndPoints != null)
                {
                    bool success = false;
                    foreach (IPEndPoint ipEndPoint in ipEndPoints)
                    {
                        success = await SendFile(ipEndPoint.Address, ipEndPoint.Port);
                        if (success)
                        {
                            break;
                        }
                    }

                    if (!success)
                    {
                        throw new InvalidOperationException("Could not send file to any of the provided endpoints.");
                    }
                }
                else
                {
                    MessageDialog.ShowLocalized("Message.Content.FileTransferDeclined", MessageDialogTitle.Warning, MessageDialogButton.Ok, MessageDialogIcon.Warning);
                }
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"File I/O error: {ex.Message}");
                MessageDialog.ShowLocalized("Message.Content.CouldNotSendFile", MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error: {ex.Message}");
                MessageDialog.ShowLocalized("Message.Content.CouldNotSendFile", MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
            }
        }

        public async Task<bool> SendFile(IPAddress ipAddress, int port)
        {
            if (_viewModel?.TargetComputer == null || _viewModel?.File == null)
            {
                throw new InvalidOperationException("Invalid target computer or file.");
            }

            try
            {
                bool success = await _fileSharing.SendFileAsync(_viewModel.File, ipAddress, port);
                if (success)
                {
                    _viewModel.IsFileSent = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"File I/O error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error: {ex.Message}");
                throw;
            }
            finally
            {
                _viewModel.IsSendingFile = false;
            }
        }
        #endregion
    }
}
