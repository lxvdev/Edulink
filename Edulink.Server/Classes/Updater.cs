using Edulink.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Edulink.Classes
{
    public class OpenUpdater
    {
        private const string RepoOwner = "lxvdev";
        private const string RepoName = "Edulink";

        private static readonly ReleaseDetails.AppTypes AppType = GetAppTypeFromString(Properties.Resources.AppType);

        private static ReleaseDetails.AppTypes GetAppTypeFromString(string appType)
        {
            return Enum.TryParse(appType, true, out ReleaseDetails.AppTypes parsedType)
                ? parsedType
                : throw new ArgumentException($"Invalid AppType in resources: {appType}");
        }

        private static readonly string CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private const string UpdateUrl = "https://api.github.com";
        //private const string UpdateUrl = "http://127.0.0.1:5000"; // For local testing

        public static async Task<ReleaseDetails> GetLatestVersionAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                    string url = $"{UpdateUrl}/repos/{RepoOwner}/{RepoName}/releases";
                    HttpResponseMessage response = await client.GetAsync(url);

                    response.EnsureSuccessStatusCode();

                    JArray releases = JArray.Parse(await response.Content.ReadAsStringAsync());

                    ReleaseDetails latestRelease = releases
                        .Select(release => new ReleaseDetails
                        {
                            Tag = (string)release["tag_name"],
                            Name = (string)release["name"],
                            Body = (string)release["body"],
                            ReleasesUrl = (string)release["html_url"],
                            DownloadUrl = GetDownloadUrl(release),
                            Version = ParseVersion((string)release["tag_name"]),
                            AppType = GetAppTypeFromTag((string)release["tag_name"])
                        })
                        .Where(release => release.AppType == AppType)
                        .OrderByDescending(release => release.Version)
                        .FirstOrDefault();

                    return latestRelease;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching latest version: {ex.Message}");
            }
        }

        public static bool IsUpdateAvailable(ReleaseDetails latestVersion)
        {
            if (latestVersion.Version == null) return false;
            Version latest = latestVersion.Version;
            Version current = new Version(CurrentVersion);
            return latest > current;
        }

        private static string GetDownloadUrl(JToken release)
        {
            return release["assets"]?
                .FirstOrDefault(asset => ((string)asset["name"]).EndsWith(".exe", StringComparison.OrdinalIgnoreCase))?["browser_download_url"]
                ?.ToString();
        }

        private static ReleaseDetails.AppTypes GetAppTypeFromTag(string tag)
        {
            return tag.EndsWith("-client", StringComparison.OrdinalIgnoreCase) ? ReleaseDetails.AppTypes.Client
                 : tag.EndsWith("-server", StringComparison.OrdinalIgnoreCase) ? ReleaseDetails.AppTypes.Server
                 : ReleaseDetails.AppTypes.None;
        }

        private static Version ParseVersion(string tag)
        {
            string versionPart = tag.Split('-')[0].TrimStart('v');
            return Version.TryParse(versionPart, out Version version) ? version : new Version(0, 0, 0, 0);
        }
    }
}
