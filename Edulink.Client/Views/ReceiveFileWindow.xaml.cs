using Edulink.Classes;
using Edulink.Communication;
using Edulink.Communication.Models;
using Edulink.Converters;
using Edulink.Models;
using Edulink.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace Edulink.Views
{
    /// <summary>
    /// Interaction logic for ReceiveFileWindow.xaml
    /// </summary>
    public partial class ReceiveFileWindow : Window
    {
        private ReceiveFileWindowViewModel _viewModel;

        private FileSharing _fileSharing;

        private readonly int _port = App.SettingsManager.Settings.FileSharingPort;

        public ReceiveFileWindow(Computer sourceComputer, string fileName, long fileLength)
        {
            InitializeComponent();
            _viewModel = new ReceiveFileWindowViewModel(sourceComputer, fileName, fileLength, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\Edulink Transfers");
            _viewModel.RequestAccept += ViewModel_RequestAcceptAsync;
            _fileSharing = new FileSharing();
            DataContext = _viewModel;
        }

        private async void ViewModel_RequestAcceptAsync(object sender, EventArgs e)
        {
            // TODO: Handle accept
            if (App.Client.Connected)
            {
                IEnumerable<IPEndPoint> ipEndPoints = IPAddressProvider.GetIPAddresses().Select(ip => new IPEndPoint(ip, _port));

                // Start the server asynchronously
                await Task.Run(() => _fileSharing.StartServer(_port));

                // Ensure the save directory exists
                if (!Directory.Exists(_viewModel.SaveDirectory))
                {
                    Directory.CreateDirectory(_viewModel.SaveDirectory);
                }

                // Send the command asynchronously
                await App.Client.Helper.SendCommandAsync(new EdulinkCommand()
                {
                    Command = Commands.ResponseSendFile.ToString(),
                    Parameters = new Dictionary<string, string>()
                    {
                        { "Accepted", true.ToString() },
                        { "TargetComputer", JsonConvert.SerializeObject(_viewModel.SourceComputer) },
                        { "IP", JsonConvert.SerializeObject(ipEndPoints, new IPEndPointConverter()) }
                    }
                });

                // Receive the file asynchronously
                await _fileSharing.ReceiveFileAsync(_viewModel.SaveDirectory, _viewModel.FileName);
            }
        }
    }
}
