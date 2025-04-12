using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Edulink.Classes
{
    public class IPAddressProvider
    {
        private static readonly List<string> _ignoredNics = new List<string> { "Hyper-V", "virtual", "VMware", "VirtualBox", "vbox" };

        public static List<IPAddress> GetIPAddresses()
        {
            List<IPAddress> ipList = new List<IPAddress>();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Ignore loopback, disconnected, and virtual adapters
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    nic.OperationalStatus != OperationalStatus.Up ||
                    _ignoredNics.Any(keyword => nic.Description.ToLower().Contains(keyword)))
                {
                    continue;
                }

                // Collect all IPv4 addresses from the current interface
                IEnumerable<IPAddress> ipAddresses = nic.GetIPProperties().UnicastAddresses
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.Address);

                ipList.AddRange(ipAddresses);
            }

            return ipList;
        }
    }
}