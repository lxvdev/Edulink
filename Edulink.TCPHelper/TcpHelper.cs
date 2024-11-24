using Edulink.Classes;
using Edulink.TCPHelper.Classes;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Edulink.TCPHelper
{
    public class TcpHelper : IDisposable
    {
        public TcpClient Client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        public TcpHelper(TcpClient tcpClient)
        {
            Client = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            _stream = Client.GetStream();
            //_bufferSize = buffer > 0 ? buffer : throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer size must be greater than 0
            _reader = new StreamReader(_stream, Encoding.UTF8);
            _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
        }

        public async Task SendCommandAsync(EdulinkCommand command)
        {
            await _sendLock.WaitAsync();
            try
            {
                await _writer.WriteLineAsync(command.ToString());
                await _writer.WriteLineAsync("END");
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public async Task<EdulinkCommand> ReceiveCommandAsync(TimeSpan? timeout = null)
        {
            StringBuilder commandBuilder = new StringBuilder();
            string line;

            using (var cts = new CancellationTokenSource())
            {
                if (timeout.HasValue)
                {
                    cts.CancelAfter(timeout.Value);
                }

                try
                {
                    while ((line = await _reader.ReadLineAsync().WithCancellation(cts.Token)) != null)
                    {
                        if (line == "END")
                        {
                            break;
                        }
                        commandBuilder.AppendLine(line);
                    }
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }

            return new EdulinkCommand(commandBuilder.ToString());
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream?.Dispose();
                _reader?.Dispose();
                _writer?.Dispose();
                Client?.Dispose();
            }
        }
    }
}
