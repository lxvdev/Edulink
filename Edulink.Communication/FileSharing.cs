using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Edulink.Communication
{
    public class FileSharing
    {
        private readonly TimeSpan _timeoutTime = TimeSpan.FromSeconds(20);

        public event EventHandler<long> ProgressChanged;
        public event EventHandler<bool> ConnectionChanged;

        private const int BufferSize = 8192;

        private TcpClient _tcpClient;
        private TcpListener _tcpListener;

        public bool StartServer(int port)
        {
            if (port < 1 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");

            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, port);
                _tcpListener.Start();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SendFileAsync(FileInfo fileInfo, IPAddress ipAddress, int port)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            if (ipAddress == null)
                throw new ArgumentNullException(nameof(ipAddress));

            if (port < 1 || port > 65535)
                throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");

            _tcpClient = new TcpClient();

            Task connectTask = _tcpClient.ConnectAsync(ipAddress, port);
            if (await Task.WhenAny(connectTask, Task.Delay(_timeoutTime)) != connectTask)
            {
                Debug.WriteLine("Connection timed out");
                _tcpClient = null;
                return false;
            }

            ConnectionChanged?.Invoke(this, true);

            try
            {
                using (NetworkStream networkStream = _tcpClient.GetStream())
                using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    long totalBytes = fileInfo.Length;
                    long sentBytes = 0;
                    byte[] buffer = new byte[BufferSize];

                    int bytesRead;
                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await networkStream.WriteAsync(buffer, 0, bytesRead);
                        sentBytes += bytesRead;

                        ProgressChanged?.Invoke(this, sentBytes);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return false;
            }
            finally
            {
                _tcpClient.Close();
                _tcpClient = null;
            }
        }

        public async Task<bool> ReceiveFileAsync(string saveDirectory, string fileName)
        {
            if (string.IsNullOrWhiteSpace(saveDirectory))
                throw new ArgumentException("Save directory cannot be empty", nameof(saveDirectory));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty", nameof(fileName));

            Task<TcpClient> acceptTask = _tcpListener.AcceptTcpClientAsync();
            if (await Task.WhenAny(acceptTask, Task.Delay(_timeoutTime)) != acceptTask)
            {
                Debug.WriteLine("Connection accept timed out");
                _tcpListener.Stop();
                _tcpListener = null;
                return false;
            }

            TcpClient client = acceptTask.Result;

            if (client == null)
                return false;

            ConnectionChanged?.Invoke(this, true);

            try
            {
                using (NetworkStream networkStream = client.GetStream())
                {
                    string savePath = Path.Combine(saveDirectory, fileName);

                    using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous))
                    {
                        byte[] buffer = new byte[BufferSize];
                        int bytesRead;
                        long receivedBytes = 0;

                        while ((bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            receivedBytes += bytesRead;

                            ProgressChanged?.Invoke(this, receivedBytes);
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return false;
            }
            finally
            {
                client.Close();
                _tcpListener.Stop();
                _tcpListener = null;
            }
        }
    }
}