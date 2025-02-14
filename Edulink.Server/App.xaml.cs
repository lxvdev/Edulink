using Edulink.Classes;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SettingsManager SettingsManager = new SettingsManager();
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetLanguage(SettingsManager.Settings.Language);
        }

        public static void SetLanguage(string locale = null)
        {
            string cultureName = locale ?? CultureInfo.InstalledUICulture.ToString();

            string resourcePath = GetLanguageDictionaryPath(cultureName);

            ResourceDictionary newDictionary = new ResourceDictionary();
            try
            {
                newDictionary.Source = new Uri(resourcePath, UriKind.Relative);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not find the specified language: {ex.Message}");

                resourcePath = GetLanguageDictionaryPath("en-US");
                newDictionary.Source = new Uri(resourcePath, UriKind.Relative);
                cultureName = "en-US";
            }

            ResourceDictionary existingDictionary = Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.StartsWith("..\\Languages\\"));
            if (existingDictionary != null)
            {
                Current.Resources.MergedDictionaries.Remove(existingDictionary);
            }

            Current.Resources.MergedDictionaries.Add(newDictionary);
        }

        private static string GetLanguageDictionaryPath(string cultureName)
        {
            switch (cultureName)
            {
                case "en-US":
                case "es-ES":
                case "ro-RO":
                case "pl-PL":
                    return $"..\\Languages\\{cultureName}.xaml";
                default:
                    return "..\\Languages\\en-US.xaml";
            }
        }

        public static void RestartApp()
        {
            Process.Start(ResourceAssembly.Location);
            Current.Shutdown();
        }

        public static void CloseApp()
        {
            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
