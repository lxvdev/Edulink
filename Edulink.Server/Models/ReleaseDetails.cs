using System;

namespace Edulink.Models
{
    public class ReleaseDetails
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }

        public string ReleasesUrl { get; set; }
        public string DownloadUrl { get; set; }

        public Version Version { get; set; }
        public AppTypes AppType { get; set; }

        public ReleaseDetails()
        {

        }

        public enum AppTypes
        {
            None,
            Client,
            Server
        }
    }
}
