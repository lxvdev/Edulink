using Edulink.Communication.Models;
using Edulink.Models;
using Edulink.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Data;

namespace Edulink.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public RelayCommand LinkCommand => new RelayCommand(async execute => await OpenLinkAsync(), canExecute => SelectedClients.Any());
        public RelayCommand MessageCommand => new RelayCommand(async execute => await SendMessageAsync(), canExecute => SelectedClients.Any());
        public RelayCommand ViewDesktopCommand => new RelayCommand(async execute => await ViewDesktopAsync(), canExecute => SelectedClients.Any());
        public RelayCommand RestartApplicationCommand => new RelayCommand(async execute => await RestartApplicationAsync(), canExecute => SelectedClients.Any());
        public RelayCommand ShutdownCommand => new RelayCommand(async execute => await ShutdownComputerAsync(), canExecute => SelectedClients.Any());
        public RelayCommand RestartCommand => new RelayCommand(async execute => await RestartComputerAsync(), canExecute => SelectedClients.Any());
        public RelayCommand LockscreenCommand => new RelayCommand(async execute => await LockscreenAsync(), canExecute => SelectedClients.Any());
        public RelayCommand UpdateCommand => new RelayCommand(async execute => await UpdateAsync(), canExecute => SelectedClients.Any());

        public MainWindowViewModel(Server server)
        {
            _clients.CollectionChanged += Clients_CollectionChanged;
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;

            ApplySorting();
            StartPreviewUpdates();
        }

        // Clients
        private ObservableCollection<Client> _clients = new ObservableCollection<Client>();
        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set
            {
                _clients = value;
                OnPropertyChanged(nameof(Clients));
            }
        }

        public bool IsBroadcast
        {
            get => Clients.Any() && Clients.Where(client => client.IsVisible).Any() && Clients.Where(client => client.IsVisible).All(client => client.IsSelected);
            set
            {
                Clients.Where(client => client.IsVisible).ToList().ForEach(client => client.IsSelected = value);
                OnPropertyChanged(nameof(IsBroadcast));
                OnPropertyChanged(nameof(SelectedClients));
            }
        }

        public List<Client> SelectedClients
        {
            get => Clients.Where(client => client.IsVisible && client.IsSelected).ToList();
            set
            {
                foreach (Client client in Clients)
                {
                    client.IsSelected = value.Any(selected => selected == client);
                }
                OnPropertyChanged(nameof(SelectedClients));
                OnPropertyChanged(nameof(IsBroadcast));
            }
        }

        public bool HasClients => Clients.Any();

        public int ClientsCount => Clients.Count;

        // Sorting
        public ICollectionView ClientsCollectionView => CollectionViewSource.GetDefaultView(Clients);

        private string _sortProperty = nameof(Client.Name);
        public string SortProperty
        {
            get => _sortProperty;
            set
            {
                _sortProperty = value;
                OnPropertyChanged();
                ApplySorting();
            }
        }

        private ListSortDirection _sortDirection = ListSortDirection.Ascending;
        public ListSortDirection SortDirection
        {
            get => _sortDirection;
            set
            {
                _sortDirection = value;
                OnPropertyChanged();
                ApplySorting();
            }
        }

        private void ApplySorting()
        {
            ClientsCollectionView.SortDescriptions.Clear();
            if (!string.IsNullOrEmpty(SortProperty))
            {
                ClientsCollectionView.SortDescriptions.Add(new SortDescription(SortProperty, SortDirection));
            }
        }

        // Preview updates
        // TODO: Stop or start preview timer instantly on setting change
        private Timer _previewTimer;
        public void StartPreviewUpdates()
        {
            if (App.SettingsManager.Settings.PreviewEnabled && App.SettingsManager.Settings.PreviewFrequency > 0)
            {
                _previewTimer = new Timer(PreviewTimerCallback, null, 0, (int)App.SettingsManager.Settings.PreviewFrequency);
            }
        }

        private Rectangle DesktopPreviewResolution { get; } = new Rectangle(0, 0, 240, 115);
        private void PreviewTimerCallback(object state)
        {
            _ = Task.Run(async () =>
            {
                await Task.WhenAll(Clients
                    .Where(client => client.IsVisible)
                    .Select(async client => await RequestPreview(client)));
            });
        }

        private async Task RequestPreview(Client client)
        {
            if (App.SettingsManager.Settings.PreviewEnabled && !client.IsExcludedFromPreview)
            {
                await client.Helper.SendCommandAsync(new EdulinkCommand
                {
                    Command = "PREVIEW",
                    Parameters = new Dictionary<string, string>
                    {
                        { "Width", DesktopPreviewResolution.Width.ToString() },
                        { "Height", DesktopPreviewResolution.Height.ToString() }
                    }
                });
            }
        }

        // Property updates
        private void Clients_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasClients));

            if (e.NewItems != null)
            {
                foreach (Client client in e.NewItems)
                {
                    client.PropertyChanged += Client_PropertyChanged;
                    _ = Task.Run(async () => await RequestPreview(client));
                }
            }

            if (e.OldItems != null)
            {
                foreach (Client client in e.OldItems)
                {
                    client.PropertyChanged -= Client_PropertyChanged;
                }
            }

            OnPropertyChanged(nameof(IsBroadcast));
            OnPropertyChanged(nameof(SelectedClients));
            OnPropertyChanged(nameof(ClientsCount));
        }

        private void Client_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Client.IsSelected) || e.PropertyName == nameof(Client.IsVisible))
            {
                OnPropertyChanged(nameof(IsBroadcast));
                OnPropertyChanged(nameof(SelectedClients));
            }
        }

        // Handle connection/disconnection
        private void Server_ClientConnected(object sender, Server.ClientConnectedEventArgs e)
        {
            _clients.Add(e.Client);
        }

        private void Server_ClientDisconnected(object sender, Server.ClientDisconnectedEventArgs e)
        {
            _clients.Remove(e.Client);
        }

        // Commands
        private async Task<bool> SendCommandAsync(EdulinkCommand command, List<Client> clients = null)
        {
            if (clients == null || !clients.Any())
            {
                MessageDialog.Show((string)Application.Current.TryFindResource("Message.Content.NoClientsSelected"),
                                   MessageDialogTitle.Error,
                                   MessageDialogButton.Ok, MessageDialogIcon.Error);
                return false;
            }

            foreach (Client client in clients)
            {
                await client.Helper.SendCommandAsync(new EdulinkCommand { Command = command.Command, Parameters = command.Parameters });
            }

            return true;
        }

        private async Task OpenLinkAsync()
        {
            InputDialogResult urlInputDialogResult = InputDialog.Show((string)Application.Current.TryFindResource("Input.Content.OpenLink"),
                                                                      (string)Application.Current.TryFindResource("Input.Title.OpenLink"));

            if (urlInputDialogResult == InputDialogButtonResult.Ok && !string.IsNullOrEmpty(urlInputDialogResult.ReplyResult))
            {
                await SendCommandAsync(
                    new EdulinkCommand
                    {
                        Command = "LINK",
                        Parameters = {
                            { "URL", PrepareUrl(urlInputDialogResult.ReplyResult) }
                        }
                    }, SelectedClients);
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

        private async Task SendMessageAsync()
        {
            InputDialogResult messageInputDialog = InputDialog.Show((string)Application.Current.TryFindResource("Input.Content.SendMessage"),
                                                                    (string)Application.Current.TryFindResource("Input.Title.SendMessage"));

            if (messageInputDialog == InputDialogButtonResult.Ok && !string.IsNullOrEmpty(messageInputDialog.ReplyResult))
            {
                await SendCommandAsync(new EdulinkCommand
                {
                    Command = "MESSAGE",
                    Parameters = {
                        { "Message", messageInputDialog.ReplyResult }
                    }
                }, SelectedClients);
            }
        }

        private async Task ViewDesktopAsync()
        {
            foreach (Client client in SelectedClients)
            {
                await RequestDesktopPreviewAsync(client);
            }
        }

        public List<DesktopPreviewDialog> openDesktopPreviewDialogs = new List<DesktopPreviewDialog>();
        private async Task RequestDesktopPreviewAsync(Client client)
        {
            DesktopPreviewDialog existingDialog = openDesktopPreviewDialogs.FirstOrDefault(dialog => dialog.Client == client);

            if (existingDialog != null)
            {
                existingDialog.Focus();
            }
            else
            {
                DesktopPreviewDialog desktopPreviewDialog = new DesktopPreviewDialog(client);
                openDesktopPreviewDialogs.Add(desktopPreviewDialog);
                desktopPreviewDialog.Closed += (s, _) => openDesktopPreviewDialogs.Remove(desktopPreviewDialog);
                desktopPreviewDialog.Show();
            }

            await client.Helper.SendCommandAsync(new EdulinkCommand { Command = "DESKTOP" });
        }

        private async Task RestartApplicationAsync()
        {
            await SendCommandAsync(new EdulinkCommand { Command = "RESTARTAPP" }, SelectedClients);
        }

        private async Task ShutdownComputerAsync()
        {
            await SendCommandAsync(new EdulinkCommand { Command = "SHUTDOWN" }, SelectedClients);
        }

        private async Task RestartComputerAsync()
        {
            await SendCommandAsync(new EdulinkCommand { Command = "RESTART" }, SelectedClients);
        }

        private async Task LockscreenAsync()
        {
            await SendCommandAsync(new EdulinkCommand { Command = "LOCKSCREEN" }, SelectedClients);
        }

        private async Task UpdateAsync()
        {
            await SendCommandAsync(new EdulinkCommand { Command = "UPDATE" }, SelectedClients);
        }
    }
}
