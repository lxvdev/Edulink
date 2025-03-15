using System;

namespace Edulink.Models
{
    public class Commands
    {
        public static readonly Command Link = new Command("LINK", "2.0.0.0");
        public static readonly Command Message = new Command("MESSAGE", "2.0.0.0");
        public static readonly Command ViewDesktop = new Command("DESKTOP", "2.0.0.0");
        public static readonly Command RestartApplication = new Command("RESTARTAPP", "2.0.0.0");
        public static readonly Command Shutdown = new Command("SHUTDOWN", "2.0.0.0");
        public static readonly Command Restart = new Command("RESTART", "2.0.0.0");
        public static readonly Command LockScreen = new Command("LOCKSCREEN", "2.0.0.0");
        public static readonly Command LogOff = new Command("LOGOFF", "2.0.0.0");
        public static readonly Command RenameComputer = new Command("RENAME", "2.0.0.0");
        public static readonly Command Update = new Command("UPDATE", "2.0.0.0");
        public static readonly Command ResetPassword = new Command("RESETPASSWORD", "2.0.0.0");
        public static readonly Command BlockInput = new Command("BLOCKINPUT", "2.0.0.0");
        public static readonly Command Preview = new Command("PREVIEW", "2.0.0.0");

        public static readonly Command ToggleMute = new Command("TOGGLEMUTE", "2.0.1.0");

        public static readonly Command ComputerList = new Command("COMPUTERLIST", "2.1.0.0");
        public static readonly Command RequestSendFile = new Command("REQUESTSENDFILE", "2.1.0.0");
        public static readonly Command ResponseSendFile = new Command("RESPONSESENDFILE", "2.1.0.0");

        public static readonly Command Connect = new Command("CONNECT", "2.0.0.0");
        public static readonly Command Disconnect = new Command("DISCONNECT", "2.0.0.0");
    }

    public class Command
    {
        public string Name { get; }
        public Version MinimumVersion { get; }

        public Command(string name)
        {
            Name = name;
        }

        public Command(string name, string minimumVersion)
        {
            Name = name;
            MinimumVersion = new Version(minimumVersion);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
