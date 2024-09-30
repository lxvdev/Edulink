using System;
using System.Security.Principal;

namespace AdminVerifier
{
    internal class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0 || args[0] != "verify")
            {
                Console.WriteLine("This tool is not meant to be run directly.");
                Console.ReadKey();
                return 2;
            }

            if (IsAdministrator())
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
        static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}
