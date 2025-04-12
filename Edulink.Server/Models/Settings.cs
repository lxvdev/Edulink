namespace Edulink.Models
{
    public class Settings
    {
        public int Port { get; set; } = 7153;
        public int FileSharingPort { get; set; } = 8073;
        public bool DisconnectionNotificationEnabled { get; set; } = true;
        public bool FileSharingStudents { get; set; } = true;
        public bool FileSharingTeacher { get; set; } = true;
        public bool PreviewEnabled { get; set; } = true;
        public double PreviewFrequency { get; set; } = 5000;
        public bool CheckForUpdates { get; set; } = true;
        public string Language { get; set; } = null;
        public string Theme { get; set; } = "Auto";
    }
}
