using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;

namespace Edulink.Classes
{
    public class SettingsManager
    {
        private readonly string _appName;
        private readonly string _appDataFolder;
        private readonly string _settingsFile;

        public AppSettings Settings { get; private set; }

        public SettingsManager(string fileName = null, string settingsFolder = null)
        {
            _appName = Assembly.GetExecutingAssembly().GetName().Name;
            _appDataFolder = Path.Combine(settingsFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _appName);
            _settingsFile = Path.Combine(_appDataFolder, fileName ?? "settings.xml");

            Load();
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_settingsFile))
                {
                    using (var reader = new StreamReader(_settingsFile))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                        Settings = (AppSettings)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    Settings = new AppSettings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}");
                Settings = Settings ?? new AppSettings();
            }
        }

        public void Save()
        {
            try
            {
                if (!Directory.Exists(_appDataFolder))
                {
                    Directory.CreateDirectory(_appDataFolder);
                }

                using (var writer = new StreamWriter(_settingsFile))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                    serializer.Serialize(writer, Settings);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}");
            }
        }

        public void Reset()
        {
            Settings = new AppSettings();
            Save();
            Console.WriteLine("Settings have been reset to default values.");
        }

        public class AppSettings
        {
            public int Port { get; set; } = 7153;
            public string Language { get; set; } = null;
        }
    }
}
