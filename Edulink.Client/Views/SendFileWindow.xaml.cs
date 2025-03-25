using Edulink.Communication.Models;
using Edulink.Models;
using Edulink.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public SendFileWindow()
        {
            InitializeComponent();
            _viewModel = new SendFileWindowViewModel();
            DataContext = _viewModel;

            App.Client.ConnectionStatusChanged += async (sender, e) => await Client_ConnectionStatusChangedAsync(sender, e);

            _ = RequestComputerList();
        }

        private async Task Client_ConnectionStatusChangedAsync(object sender, bool e)
        {
            if (!App.Client.Connected) _viewModel.SetReceivingStatus(false);

            await RequestComputerList();
        }

        private async Task RequestComputerList()
        {
            try
            {
                if (App.Client.Connected)
                {
                    _viewModel.SetReceivingStatus(true);
                    await App.Client.Helper?.SendCommandAsync(new EdulinkCommand() { Command = Commands.ComputerList.ToString() });
                }
                else
                {
                    _viewModel.SetReceivingStatus(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not request computer list: {ex.Message}");
                _viewModel.SetReceivingStatus(false);
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

        // TODO: Create a "File" model
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

        private void SetFile(string filePath)
        {
            if (filePath == null) return;

            _viewModel.SetFile(new FileInfo(filePath));
        }
    }
}
