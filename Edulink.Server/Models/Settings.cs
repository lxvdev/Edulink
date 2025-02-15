namespace Edulink.Server.Models
{
    public class Settings
    {
        public int Port { get; set; } = 7153;
        public bool PreviewEnabled { get; set; } = true;
        public double PreviewFrequency { get; set; } = 5000;
        public string Language { get; set; } = null;
    }
}
