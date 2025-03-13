using Edulink.Migrator.Migrations;
using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Principal;

namespace Edulink.Migrator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Edulink Migrator";
            if (!IsAdministrator())
            {
                Console.WriteLine("Please run this program as administrator.");
                return;
            }
            MainMenu();
        }

        private static void MainMenu()
        {
            Console.Clear();
            Console.WriteLine("1. Fully remove Edulink 1");
            Console.WriteLine("2. Migrate settings from Edulink 1 to Edulink 2");
            Console.Write(">> ");

            int.TryParse(Console.ReadLine(), out int option);

            if (option == 0) { MainMenu(); }

            Console.Clear();

            switch (option)
            {
                case 1:
                    Edulink1.Remove();
                    break;
                case 2:
                    if (Edulink1.MigrateSettings() == null)
                    {
                        Console.WriteLine("No settings found to migrate.");
                    }
                    break;
                default:
                    break;
            }
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static List<string> GetWindowsUsers()
        {
            List<string> userList = new List<string>();
            try
            {
                SelectQuery query = new SelectQuery("Win32_UserAccount");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                foreach (ManagementObject user in searcher.Get())
                {
                    userList.Add(user["Name"].ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving users: " + ex.Message);
            }

            return userList;
        }

        static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
