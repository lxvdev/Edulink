namespace Edulink.Models
{
    public class Settings
    {
        public string Name { get; set; } = null;
        public string IPAddress { get; set; } = null;
        public int Port { get; set; } = 7153;
        public int FileSharingPort { get; set; } = 8073;
        public bool CheckForUpdates { get; set; } = true;
        public string Language { get; set; } = null;
        public string Password { get; set; } = null;
        public string Theme { get; set; } = "Auto";
    }
}
