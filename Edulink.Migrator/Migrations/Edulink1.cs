using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Edulink.Migrator.Migrations
{
    internal class Edulink1
    {
        public static void Remove()
        {
            Console.Write("Removing Edulink 1... ");

            if (RemoveEdulink1(false))
            {
                Console.WriteLine("OK");
            }
            else
            {
                Console.WriteLine("FAIL");
            }

            Console.Write("Removing Edulink 1 (64-bit)... ");

            if (RemoveEdulink1(true))
            {
                Console.WriteLine("OK");
            }
            else
            {
                Console.WriteLine("FAIL");
            }

            Console.Write("Removing boot registry... ");

            if (RemoveBootRegistry())
            {
                Console.WriteLine("OK");
            }
            else
            {
                Console.WriteLine("FAIL");
            }
        }

        public static bool RemoveEdulink1(bool is64bit = false)
        {
            try
            {
                string programFilesPath = is64bit
                    ? @"C:\Program Files"
                    : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                string edulink1ClientPath = Path.Combine(programFilesPath, "Edulink.Client");
                string edulink1ServerPath = Path.Combine(programFilesPath, "Edulink.Server");

                if (Directory.Exists(edulink1ClientPath))
                {
                    Directory.Delete(edulink1ClientPath, true);
                }

                if (Directory.Exists(edulink1ServerPath))
                {
                    Directory.Delete(edulink1ServerPath, true);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool RemoveBootRegistry()
        {
            try
            {
                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (regKey.GetValue("Edulink.Client") != null)
                    {
                        regKey.DeleteValue("Edulink.Client");
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool? MigrateSettings()
        {
            List<string> users = Program.GetWindowsUsers();
            if (users.Count == 0) return true;

            List<(bool isClient, string settingsPath)> validOptions = new List<(bool isClient, string settingsPath)>();

            // Show user list
            foreach (string user in users)
            {
                string clientAppDataPath = Path.Combine(@"C:\Users", user, "AppData", "Roaming", "Edulink.Client");
                string clientSettings = Path.Combine(clientAppDataPath, "settings.xml");

                string serverAppDataPath = Path.Combine(@"C:\Users", user, "AppData", "Roaming", "Edulink.Server");
                string serverSettings = Path.Combine(serverAppDataPath, "settings.xml");

                if (File.Exists(clientSettings))
                {
                    validOptions.Add((true, clientSettings));
                    Console.WriteLine($"{validOptions.Count}. {user} (Client: {clientSettings})");
                }

                if (File.Exists(serverSettings))
                {
                    validOptions.Add((false, serverSettings));
                    Console.WriteLine($"{validOptions.Count}. {user} (Server: {serverSettings})");
                }
            }

            if (validOptions.Count == 0) return null;

            Console.WriteLine("Choose a user to migrate settings from:");
            Console.Write(">> ");

            if (!int.TryParse(Console.ReadLine(), out int option) || option < 1 || option > validOptions.Count)
            {
                MigrateSettings();
                return true;
            }

            (bool isClient, string settingsPath) = validOptions[option - 1];

            string programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), isClient ? "Edulink Client" : "Edulink Server");
            string destinationPath = Path.Combine(programDataPath, "settings.xml");

            // Start migration
            try
            {
                if (!Directory.Exists(programDataPath))
                {
                    Directory.CreateDirectory(programDataPath);
                }

                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                XDocument xmlDoc = XDocument.Load(settingsPath);
                xmlDoc.Root.Name = "Settings";
                xmlDoc.Save(destinationPath);

                if (Directory.Exists(Path.GetDirectoryName(settingsPath)))
                {
                    Directory.Delete(Path.GetDirectoryName(settingsPath), true);
                }

                Console.WriteLine($"Successfully migrated settings.");
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }
}
