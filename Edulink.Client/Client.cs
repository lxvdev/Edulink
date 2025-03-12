using Edulink.Communication;
using Edulink.Communication.Models;
using Edulink.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace Edulink
{
    public class Client : IDisposable
    {
        private TcpClient tcpClient;
        public TcpHelper Helper;
        private string ipAddress;
        private int port;
        private string name;
        public bool Connected { get; private set; } = false;

        public event EventHandler<EdulinkCommand> CommandReceived;
        public event EventHandler<bool> ConnectionStatusChanged;

        public Client(string ipAddress, int port, string name)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.name = name;
            tcpClient = new TcpClient();
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                await tcpClient.ConnectAsync(ipAddress, port);
                Helper = new TcpHelper(tcpClient);

                await Helper.SendCommandAsync(new EdulinkCommand
                {
                    Command = Commands.Connect.ToString(),
                    Parameters = new Dictionary<string, string>
                    {
                        { "Name", name },
                        { "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                        { "UpdateAvailable", App.UpdateAvailable.ToString() }
                    }
                });

                EdulinkCommand response = await Helper.ReceiveCommandAsync(TimeSpan.FromSeconds(5));
                if (response == null)
                {
                    Dispose();
                    return false;
                }

                Connected = true;
                ConnectionStatusChanged?.Invoke(this, Connected);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task ListenForCommandsAsync()
        {
            while (true)
            {
                try
                {
                    EdulinkCommand command = await Helper.ReceiveCommandAsync();

                    if (command == null)
                    {
                        throw new Exception("Connection lost");
                    }

                    Console.WriteLine($"Command received: {command.Command}");
                    CommandReceived?.Invoke(this, command);
                }
                catch (Exception ex)
                {
                    Connected = false;
                    ConnectionStatusChanged?.Invoke(this, Connected);
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }

        public void Disconnect()
        {
            //await Helper.SendCommandAsync(new EdulinkCommand
            //{
            //    Command = Commands.Disconnect
            //});
            Dispose();
        }

        public void Dispose()
        {
            Helper?.Dispose();
            Connected = false;
            ConnectionStatusChanged?.Invoke(this, Connected);
        }
    }
}