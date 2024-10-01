using Edulink.TCPHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpListener listener;
        private List<ClientInfo> clients = new List<ClientInfo>();

        SettingsWindow settingsWindow;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            settingsWindow = new SettingsWindow();
            InitializeListBox();
            if (App.configManager.Settings.Port.ToString().Length > 4)
            {
                MessageBox.Show("Port cannot be larger than 4 numbers.");
                settingsWindow.Show();
            }
            else
            {
                await StartServerAsync();
            }
        }

        private async Task StartServerAsync()
        {
            listener = new TcpListener(IPAddress.Any, App.configManager.Settings.Port);
            listener.Start();

            var ipAddresses = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
            .Where(networkInterface => networkInterface.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
            .SelectMany(networkInterface => networkInterface.GetIPProperties().UnicastAddresses)
            .Where(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork && ip.Address.ToString() != "127.0.0.1")
            .Select(ip => ip.Address.ToString());

            string allIpAddresses = string.Join(", ", ipAddresses);

            IPAddressesTextBlock.Text = allIpAddresses;

            Console.WriteLine($"Server started on IP {allIpAddresses}. Waiting for connections...");
            try
            {
                while (true)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    HandleClient(tcpClient);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accepting Tcp Clients: {ex.Message}");
            }
        }

        private async void HandleClient(TcpClient tcpClient)
        {
            TcpHelper helper = new TcpHelper(tcpClient, 1024);

            string clientName = await helper.ReceiveTextWithTimeoutAsync(5000);
            if (string.IsNullOrEmpty(clientName))
            {
                Console.WriteLine("No name received within timeout. Closing connection.");
                helper.Close();
                return;
            }

            var clientInfo = new ClientInfo(helper, clientName);
            clients.Add(clientInfo);

            Dispatcher.Invoke(() =>
            {
                ConnectedPCsList.Items.Add(new ListBoxItem
                {
                    Content = $"{clientInfo.Name} ({clientInfo.Endpoint})",
                    DataContext = clientInfo
                });
            });

            await helper.SendTextAsync($"Welcome to the server, {clientName}!");
            Console.WriteLine($"{clientInfo.Name} ({clientInfo.Endpoint}) connected.");

            try
            {
                await ListenForCommandsAsync(clientInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client {clientInfo.Name} disconnected: {ex.Message}");
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    var itemToRemove = ConnectedPCsList.Items.Cast<ListBoxItem>()
                        .FirstOrDefault(item => item.DataContext == clientInfo);
                    if (itemToRemove != null)
                    {
                        ConnectedPCsList.Items.Remove(itemToRemove);
                    }
                });

                clients.Remove(clientInfo);
                helper.Close();
            }
        }
        private List<DesktopPreviewDialog> openDesktopPreviewDialogs = new List<DesktopPreviewDialog>();
        public async Task ListenForCommandsAsync(ClientInfo clientInfo)
        {
            while (true)
            {
                try
                {
                    (string command, string argument) = await clientInfo.Helper.ReceiveCommandAsync();

                    if (string.IsNullOrEmpty(command))
                    {
                        throw new Exception("Connection lost");
                    }

                    Console.WriteLine($"Received from {clientInfo.Name}: {command}");

                    // Handle Commands
                    switch (command)
                    {
                        case "DesktopPreview":
                            DesktopPreviewDialog existingDialog = openDesktopPreviewDialogs.FirstOrDefault(d => d.ClientInfo == clientInfo);
                            DesktopPreviewButton.SetResourceReference(ContentProperty, "ViewDesktopState2Button");
                            Bitmap image = await clientInfo.Helper.ReceiveImageAsync();
                            DesktopPreviewButton.SetResourceReference(ContentProperty, "ViewDesktopButton");
                            //DesktopPreviewButton.IsEnabled = true;
                            if (existingDialog == null)
                            {
                                DesktopPreviewDialog desktopPreviewDialog = new DesktopPreviewDialog(image, clientInfo);
                                openDesktopPreviewDialogs.Add(desktopPreviewDialog);
                                desktopPreviewDialog.Closed += (s, e) => openDesktopPreviewDialogs.Remove(desktopPreviewDialog);
                                Application.Current.Dispatcher.Invoke(() => desktopPreviewDialog.Show());
                            }
                            else
                            {
                                Application.Current.Dispatcher.Invoke(() => existingDialog.UpdateScreenshot(image));
                            }
                            break;
                        case "SendMessage":
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBoxDialog messageBoxDialog = new MessageBoxDialog(argument, clientInfo);
                                messageBoxDialog.Show();
                            });
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }

        private async void Command_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string command = button?.Tag?.ToString() ?? string.Empty;
            switch (command)
            {
                case "Link":
                    InputDialog urlInputDialog = new InputDialog(
                        (string)Application.Current.Resources["LinkDialogContent"],
                        (string)Application.Current.Resources["LinkDialogCaption"]);

                    if (urlInputDialog.ShowDialog() == true)
                    {
                        await SendCommandAsync(command, PrepareUrl(urlInputDialog.InputValue));
                    }
                    break;

                case "SendMessage":
                    InputDialog messageInputDialog = new InputDialog(
                        (string)Application.Current.Resources["SendMessageDialogContent"],
                        (string)Application.Current.Resources["SendMessageDialogCaption"]);

                    if (messageInputDialog.ShowDialog() == true)
                    {
                        await SendCommandAsync(command, messageInputDialog.InputValue);
                    }
                    break;
                case "DesktopPreview":
                    if (await SendCommandAsync(command, null, false))
                    {
                        //DesktopPreviewButton.IsEnabled = false;
                        DesktopPreviewButton.SetResourceReference(ContentProperty, "ViewDesktopState1Button");
                    }
                    break;
                case "RestartApp":
                    await SendCommandAsync(command);
                    break;
                case "Shutdown":
                    await SendCommandAsync(command);
                    break;
                case "Lockscreen":
                    await SendCommandAsync(command);
                    break;
            }
        }
        private string PrepareUrl(string url)
        {
            url = url.Trim();
            url = HttpUtility.UrlPathEncode(url);

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            // Caused some problems
            //if (!Regex.IsMatch(url, @"\.[a-z]{2,}$"))
            //{
            //    url += ".com";
            //}

            return url;
        }

        private async Task<bool> SendCommandAsync(string command, string arguments = null, bool multipleClients = true)
        {
            if (ConnectedPCsList.SelectedItems.Count > 0)
            {
                if (ConnectedPCsList.SelectedItems.Cast<ListBoxItem>().Any(item => item.DataContext.ToString() == "AllPCs"))
                {
                    if (multipleClients)
                    {
                        foreach (var client in clients)
                        {
                            await client.Helper.SendCommandAsync(command, arguments);
                        }
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("This command can't be used on all clients", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                }

                foreach (ListBoxItem selectedItem in ConnectedPCsList.SelectedItems)
                {
                    if (selectedItem is ListBoxItem item && item.DataContext is ClientInfo selectedClient)
                    {
                        await selectedClient.Helper.SendCommandAsync(command, arguments);
                    }
                }

                return true;
            }
            return false;
        }
        private void InitializeListBox()
        {
            ListBoxItem listBoxItem = new ListBoxItem
            {
                DataContext = "AllPCs",
                FontWeight = FontWeights.SemiBold,
            };

            listBoxItem.SetResourceReference(ContentProperty, "AllPCsText");

            ConnectedPCsList.Items.Add(listBoxItem);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            settingsWindow.Show();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            App.CloseApp();
        }

        private void AboutItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            App.CloseApp();
        }
    }

    public class ClientInfo
    {
        public TcpHelper Helper { get; set; }
        public string Name { get; set; }
        public string Endpoint { get; set; }

        public ClientInfo(TcpHelper helper, string name)
        {
            Helper = helper;
            Name = name;
            Endpoint = Helper.client.Client.RemoteEndPoint.ToString();
        }
    }
}
