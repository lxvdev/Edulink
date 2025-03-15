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
        private TcpClient _tcpClient;
        public TcpHelper Helper;

        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string Name { get; set; }
        public bool Connected { get; private set; } = false;

        public event EventHandler<EdulinkCommand> CommandReceived;
        public event EventHandler<bool> ConnectionStatusChanged;

        public Client(string ipAddress, int port, string name)
        {
            this.IPAddress = ipAddress;
            this.Port = port;
            this.Name = name;
        }

        public Client()
        {

        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _tcpClient = new TcpClient();

                await _tcpClient.ConnectAsync(IPAddress, Port);
                Helper = new TcpHelper(_tcpClient);

                await Helper.SendCommandAsync(new EdulinkCommand
                {
                    Command = Commands.Connect.ToString(),
                    Parameters = new Dictionary<string, string>
                {
                    { "Name", Name },
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
            _tcpClient?.Dispose();
            Connected = false;
            ConnectionStatusChanged?.Invoke(this, Connected);
        }
    }
}