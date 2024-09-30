using Edulink.Classes;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static SettingsManager configManager = new SettingsManager();
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetLanguageDictionary(configManager.Settings.Language);
        }

        public static void SetLanguageDictionary(string locale = null)
        {
            ResourceDictionary dict = new ResourceDictionary();
            if (locale != null)
            {
                try
                {
                    dict.Source = new Uri($"..\\Languages\\{locale}.xaml", UriKind.Relative);
                }
                catch (Exception)
                {
                    Console.WriteLine("Could not find the specified language.");
                }

            }
            else
            {
                switch (CultureInfo.InstalledUICulture.ToString())
                {

                    case "en-US":
                        dict.Source = new Uri("..\\Languages\\en-US.xaml", UriKind.Relative);
                        configManager.Settings.Language = CultureInfo.InstalledUICulture.ToString();
                        break;
                    case "es-ES":
                        dict.Source = new Uri("..\\Languages\\es-ES.xaml", UriKind.Relative);
                        configManager.Settings.Language = CultureInfo.InstalledUICulture.ToString();
                        break;
                    case "ro-RO":
                        dict.Source = new Uri("..\\Languages\\ro-RO.xaml", UriKind.Relative);
                        configManager.Settings.Language = CultureInfo.InstalledUICulture.ToString();
                        break;
                    case "pl-PL":
                        dict.Source = new Uri("..\\Languages\\pl-PL.xaml", UriKind.Relative);
                        configManager.Settings.Language = CultureInfo.InstalledUICulture.ToString();
                        break;
                    default:
                        dict.Source = new Uri("..\\Languages\\en-US.xaml", UriKind.Relative);
                        configManager.Settings.Language = "en-US";
                        break;
                }
            }
            Current.Resources.MergedDictionaries.Add(dict);
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
    }
}
