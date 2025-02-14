using Edulink.Models;
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

        public Settings Settings { get; set; }

        public SettingsManager(string settingsFolder = null, string fileName = null)
        {
            _appName = Assembly.GetExecutingAssembly().GetName().Name;
            _appDataFolder = settingsFolder ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _appName);
            _settingsFile = Path.Combine(_appDataFolder, fileName ?? "settings.xml");

            Load();
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_settingsFile))
                {
                    using (StreamReader reader = new StreamReader(_settingsFile))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                        Settings = (Settings)serializer.Deserialize(reader);
                    }
                }
                else
                {
                    Settings = new Settings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                Settings = Settings ?? new Settings();
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

                using (StreamWriter writer = new StreamWriter(_settingsFile))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
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
            Settings = new Settings();
            Save();
            Console.WriteLine("Settings have been reset to default values.");
        }
    }
}
