using Edulink.Server.Communication;
using Edulink.Server.Communication.Classes;
using Edulink.Server.Communication.Models;
using Edulink.Server.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Edulink.Server.Core
{
    public class Server : IDisposable
    {
        private TcpListener _listener;
        private readonly int _port;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _disposed = false;

        public Server(int port)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartServerAsync()
        {
            _listener.Start();
            Console.WriteLine($"Server started on port {_port}.");
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    TcpClient tcpClient = await _listener.AcceptTcpClientAsync().WithCancellation(_cancellationTokenSource.Token);
                    _ = HandleClient(tcpClient);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        private async Task HandleClient(TcpClient tcpClient)
        {
            Client client = null;
            TcpHelper helper = new TcpHelper(tcpClient);
            try
            {
                EdulinkCommand command = await helper.ReceiveCommandAsync(TimeSpan.FromSeconds(5));
                if (string.IsNullOrEmpty(command.Command) || !command.Command.Equals("CONNECT"))
                {
                    Console.WriteLine("Invalid or no connection command received. Closing connection.");
                    return;
                }

                client = new Client(helper, command.Parameters["Name"], command.Parameters["Version"]);

                ClientConnected?.Invoke(this, new ClientConnectedEventArgs(client));

                await helper.SendCommandAsync(new EdulinkCommand { Command = $"WELCOME" });
                Console.WriteLine($"{client.Name} ({client.Endpoint}) connected.");

                await ListenForCommandsAsync(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client {client?.Name} disconnected: {ex.Message}");
            }
            finally
            {
                client?.Dispose();
                ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(client));
            }
        }

        private async Task ListenForCommandsAsync(Client client)
        {
            while (true)
            {
                EdulinkCommand command = await client.Helper.ReceiveCommandAsync().WithCancellation(_cancellationTokenSource.Token);

                if (string.IsNullOrEmpty(command.Command))
                {
                    throw new Exception("Connection lost");
                }

                CommandReceived?.Invoke(this, new CommandReceivedEventArgs(client, command));
            }
        }

        #region Event handlers
        public event EventHandler<ClientConnectedEventArgs> ClientConnected;
        public class ClientConnectedEventArgs : EventArgs
        {
            public Client Client { get; }

            public ClientConnectedEventArgs(Client client)
            {
                Client = client;
            }
        }

        public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
        public class ClientDisconnectedEventArgs : EventArgs
        {
            public Client Client { get; }

            public ClientDisconnectedEventArgs(Client client)
            {
                Client = client;
            }
        }

        public event EventHandler<CommandReceivedEventArgs> CommandReceived;
        public class CommandReceivedEventArgs : EventArgs
        {
            public Client Client { get; }
            public EdulinkCommand Command { get; }

            public CommandReceivedEventArgs(Client client, EdulinkCommand command)
            {
                Client = client;
                Command = command;
            }
        }

        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cancellationTokenSource.Cancel();
                    _listener?.Stop();
                    _listener = null;
                    _cancellationTokenSource.Dispose();
                }
                _disposed = true;
            }
        }

        ~Server()
        {
            Dispose(false);
        }
        #endregion
    }
}
