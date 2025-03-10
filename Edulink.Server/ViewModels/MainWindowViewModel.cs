﻿using Edulink.Classes;
using Edulink.Communication.Models;
using Edulink.Core;
using Edulink.Models;
using Edulink.MVVM;
using Edulink.Views;
using Hardcodet.Wpf.TaskbarNotification;
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
using System.Windows.Data;
using System.Windows.Input;

namespace Edulink.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
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

        public int ShowMainContent => HasClients ? 1 : 0;

        public int ClientsCount => Clients.Count;

        public MainWindowViewModel(Server server)
        {
            _clients.CollectionChanged += Clients_CollectionChanged;
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;

            ApplySorting();
            StartPreviewUpdates();
        }

        #region Sorting
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
        #endregion

        #region Preview updates
        // TODO: Stop or start preview timer instantly on setting change
        private Timer _previewTimer;
        public void StartPreviewUpdates()
        {
            if (App.SettingsManager.Settings.PreviewEnabled && App.SettingsManager.Settings.PreviewFrequency > 1000)
            {
                _previewTimer = new Timer(PreviewTimerCallback, null, 0, (int)App.SettingsManager.Settings.PreviewFrequency);
            }
        }

        private Rectangle DesktopPreviewResolution { get; } = new Rectangle(0, 0, 240, 135);
        private void PreviewTimerCallback(object state)
        {
            Clients.Where(client => client.IsVisible && !client.IsExcludedFromPreview).ToList()
                   .ForEach(client => RequestPreview(client));
        }

        private void RequestPreview(Client client)
        {
            if (App.SettingsManager.Settings.PreviewEnabled)
            {
                _ = client.Helper.SendCommandAsync(new EdulinkCommand
                {
                    Command = Commands.Preview,
                    Parameters = new Dictionary<string, string>
                    {
                        { "Width", DesktopPreviewResolution.Width.ToString() },
                        { "Height", DesktopPreviewResolution.Height.ToString() }
                    }
                });
            }
        }
        #endregion

        #region Property updates
        private void Clients_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasClients));
            OnPropertyChanged(nameof(ShowMainContent));

            if (e.NewItems != null)
            {
                foreach (Client client in e.NewItems)
                {
                    client.PropertyChanged += Client_PropertyChanged;
                    // I dont like this but it works
                    //Task.Delay(200).ContinueWith(task => RequestPreview(client));
                    // I fixed it!!!
                    _ = Task.Run(() =>
                    {
                        RequestPreview(client);
                    });
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
        #endregion

        #region Handle connection/disconnection
        private void Server_ClientConnected(object sender, Server.ClientConnectedEventArgs e)
        {
            _clients.Add(e.Client);
        }

        private void Server_ClientDisconnected(object sender, Server.ClientDisconnectedEventArgs e)
        {
            _clients.Remove(e.Client);
            App.ActiveBalloonTipType = App.BalloonTipType.ComputerDisconnected;
            App.TaskbarIcon.ShowBalloonTip(LocalizedStrings.Instance["TaskbarIcon.Title.ComputerDisconnected"], string.Format(LocalizedStrings.Instance["TaskbarIcon.Content.ComputerDisconnected"], e.Client?.Name ?? "???"), BalloonIcon.Info);
        }
        #endregion

        #region Commands
        private async Task<bool> SendCommandAsync(EdulinkCommand command, List<Client> clients = null)
        {
            if (clients?.Any() != true)
            {
                MessageDialog.ShowLocalized("Message.Content.NoClientsSelected", MessageDialogTitle.Error, MessageDialogButton.Ok, MessageDialogIcon.Error);
                return false;
            }

            await Task.WhenAll(clients.Select(client =>
                    client.Helper.SendCommandAsync(new EdulinkCommand { Command = command.Command, Parameters = command.Parameters })));

            return true;
        }

        #region Link command
        public ICommand LinkCommand => new RelayCommand(execute => SendLink(), canExecute => SelectedClients.Any());
        private void SendLink()
        {
            InputDialogResult urlInputDialogResult = InputDialog.ShowLocalized("Input.Content.SendLink", LocalizedStrings.Instance["Input.Title.SendLink"]);

            if (urlInputDialogResult.ButtonResult == InputDialogButtonResult.Ok && !string.IsNullOrEmpty(urlInputDialogResult.InputResult))
            {
                _ = SendCommandAsync(new EdulinkCommand
                {
                    Command = Commands.Link,
                    Parameters =
                    {
                        { "URL", PrepareUrl(urlInputDialogResult.InputResult) }
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
        #endregion

        #region Message command
        public ICommand MessageCommand => new RelayCommand(execute => SendMessage(), canExecute => SelectedClients.Any());
        private void SendMessage()
        {
            InputDialogResult messageInputDialog = InputDialog.ShowLocalized("Input.Content.SendMessage", LocalizedStrings.Instance["Input.Title.SendMessage"]);

            if (messageInputDialog.ButtonResult == InputDialogButtonResult.Ok && !string.IsNullOrEmpty(messageInputDialog.InputResult))
            {
                _ = SendCommandAsync(new EdulinkCommand
                {
                    Command = Commands.Message,
                    Parameters = {
                        { "Message", messageInputDialog.InputResult }
                    }
                }, SelectedClients);
            }
        }
        #endregion

        #region View desktop command
        public ICommand DesktopCommand => new RelayCommand(execute => ViewDesktop(), canExecute => SelectedClients.Any());
        private void ViewDesktop()
        {
            SelectedClients.ForEach(async client => await RequestDesktopAsync(client));
        }

        public List<DesktopDialog> openDesktopDialogs = new List<DesktopDialog>();
        private async Task RequestDesktopAsync(Client client)
        {
            DesktopDialog existingDialog = openDesktopDialogs.FirstOrDefault(dialog => dialog.Client == client);

            if (existingDialog != null)
            {
                existingDialog.Focus();
            }
            else
            {
                DesktopDialog desktopPreviewDialog = new DesktopDialog(client);
                openDesktopDialogs.Add(desktopPreviewDialog);
                desktopPreviewDialog.Closed += (s, _) => openDesktopDialogs.Remove(desktopPreviewDialog);
                desktopPreviewDialog.Show();
            }

            await client.Helper.SendCommandAsync(new EdulinkCommand { Command = Commands.Desktop });
        }
        #endregion

        public ICommand SimpleCommand => new RelayCommand(execute => _ = SendCommandAsync(new EdulinkCommand { Command = (string)execute }, SelectedClients), canExecute => SelectedClients.Any());

        public ICommand BlockInputCommand => new RelayCommand(execute => HandleBlockInput(bool.Parse((string)execute)), canExecute => SelectedClients.Any());
        private void HandleBlockInput(bool block)
        {
            _ = SendCommandAsync(new EdulinkCommand
            {
                Command = Commands.BlockInput,
                Parameters = new Dictionary<string, string>
                {
                    { "Block", block.ToString() }
                }
            }, SelectedClients);
        }

        public ICommand ExcludePreviewCommand => new RelayCommand(execute => ExcludePreview(bool.Parse((string)execute)));
        private void ExcludePreview(bool exclude)
        {
            foreach (Client client in SelectedClients.Where(c => c.IsExcludedFromPreview == !exclude))
            {
                client.IsExcludedFromPreview = exclude;
                if (exclude)
                {
                    client.Preview = null;
                }
                else
                {
                    RequestPreview(client);
                }
            }
        }

        public ICommand RenameCommand => new RelayCommand(execute => Rename(), canExecute => SelectedClients.Any());
        private void Rename()
        {
            InputDialogResult renameInputDialog = InputDialog.ShowLocalized("Input.Content.RenameComputer", LocalizedStrings.Instance["Input.Title.RenameComputer"]);

            if (renameInputDialog.ButtonResult == InputDialogButtonResult.Ok && !string.IsNullOrEmpty(renameInputDialog.InputResult))
            {
                _ = SendCommandAsync(new EdulinkCommand
                {
                    Command = Commands.RenameComputer,
                    Parameters = {
                        { "Name", renameInputDialog.InputResult }
                    }
                }, SelectedClients);
            }
        }

        #endregion
    }
}
