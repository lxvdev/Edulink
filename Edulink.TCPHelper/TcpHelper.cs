using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Edulink.TCPHelper
{
    public class TcpHelper
    {
        public TcpClient client;
        private NetworkStream stream;
        private int bufferSize;

        public TcpHelper(TcpClient tcpClient, int buffer)
        {
            client = tcpClient;
            stream = client.GetStream();
            bufferSize = buffer;
        }

        // Sending and receiving text
        public async Task SendTextAsync(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't send text: {ex.Message}");
            }
        }
        public async Task<string> ReceiveTextAsync()
        {
            try
            {
                byte[] buffer = new byte[bufferSize];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                return message;
            }
            catch (IOException)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't receive text: {ex.Message}");
            }
        }
        public async Task<string> ReceiveTextWithTimeoutAsync(int timeoutMilliseconds)
        {
            try
            {
                Task<string> receiveTask = ReceiveTextAsync();
                Task delayTask = Task.Delay(timeoutMilliseconds);

                Task completedTask = await Task.WhenAny(receiveTask, delayTask);

                if (completedTask == receiveTask)
                {
                    return await receiveTask;
                }
                else
                {
                    // If the timeout occurred
                    return null;
                }
            }
            catch (IOException)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't receive text: {ex.Message}");
            }
        }

        // Sending and receiving commands
        public async Task SendCommandAsync(string command, string argument = null)
        {
            try
            {
                string message = argument == null ? command : $"{command}\u001E{argument}";

                await SendTextAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Couldn't send command: {ex.Message}");
            }
        }
        public async Task<(string command, string argument)> ReceiveCommandAsync()
        {
            try
            {
                string message = await ReceiveTextAsync();

                string[] parts = message?.Split('\u001E');

                if (parts != null && parts.Length > 0)
                {
                    string command = parts[0];
                    string argument = parts.Length > 1 ? parts[1] : null;
                    return (command, argument);
                }
                else
                {
                    throw new Exception("Received data is not in the expected format.");
                }
            }
            catch (IOException)
            {
                return (null, null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Couldn't receive command: {ex.Message}");
            }
        }

        // Sending and receiving images
        public async Task SendImageAsync(Bitmap screenshot)
        {
            byte[] imageBytes = ImageToByteArray(screenshot);

            byte[] lengthPrefix = BitConverter.GetBytes(imageBytes.Length);
            await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);

            await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
            await stream.FlushAsync();
        }
        public async Task<Bitmap> ReceiveImageAsync()
        {
            byte[] lengthPrefix = new byte[sizeof(int)];
            await stream.ReadAsync(lengthPrefix, 0, 4);
            int imageLength = BitConverter.ToInt32(lengthPrefix, 0);

            byte[] imageBytes = new byte[imageLength];
            int totalBytesRead = 0;

            while (totalBytesRead < imageLength)
            {
                int bytesRead = await stream.ReadAsync(imageBytes, totalBytesRead, imageLength - totalBytesRead);
                if (bytesRead == 0)
                {
                    throw new Exception("Client disconnected during image transfer");
                }
                totalBytesRead += bytesRead;
            }

            return ByteArrayToImage(imageBytes);
        }

        // Converting images
        private byte[] ImageToByteArray(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
        private Bitmap ByteArrayToImage(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                return new Bitmap(ms);
            }
        }

        public void Close()
        {
            stream?.Close();
            client?.Close();
        }
    }
}
