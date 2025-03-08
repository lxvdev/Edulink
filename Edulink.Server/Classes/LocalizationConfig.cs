using Edulink.Languages;
using System.Reflection;

namespace Edulink.Classes
{
    public static class LocalizationConfig
    {
        public static string DesignCulture { get; set; } = "en-US";
        public static string DefaultAssembly { get; set; } = Assembly.GetExecutingAssembly().FullName;
        public static string DefaultDictionary { get; set; } = nameof(Strings);
    }
}
