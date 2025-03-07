using Edulink.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
            _appDataFolder = settingsFolder ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), _appName);
            _settingsFile = Path.Combine(_appDataFolder, fileName ?? "settings.xml");

            Load();
        }

        public void Load(string path = null)
        {
            string settingsFile = path ?? _settingsFile;
            try
            {
                if (File.Exists(settingsFile))
                {
                    using (StreamReader reader = new StreamReader(settingsFile))
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
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                Settings = Settings ?? new Settings();
            }
        }

        public void Save(bool noRetry = false)
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
                if (noRetry)
                {
                    return;
                }
                Debug.WriteLine($"Error saving settings: {ex.Message}");
                string tempPath = Path.Combine(Path.GetTempPath(), Assembly.GetExecutingAssembly().GetName().Name);
                string tempSettings = Path.Combine(tempPath, "settings_temp.xml");

                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }

                using (StreamWriter writer = new StreamWriter(tempSettings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    serializer.Serialize(writer, Settings);
                }

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    Arguments = $"--apply-settings \"{tempSettings}\"",
                    UseShellExecute = true,
                    Verb = "runas"
                };

                Process.Start(psi);
            }
        }

        public void Reset()
        {
            Settings = new Settings();
            Save();
            Debug.WriteLine("Settings have been reset to default values.");
        }
    }
}
