using Edulink.Languages;
using System.Reflection;
using WPFLocalizeExtension.Engine;

namespace Edulink.Classes
{
    public class LocalizedStrings
    {
        public static LocalizedStrings Instance { get; } = new LocalizedStrings();

        public string this[string key]
        {
            get
            {
                return LocalizeDictionary.Instance.GetLocalizedObject(Assembly.GetExecutingAssembly().GetName().Name, nameof(Strings), key, LocalizeDictionary.Instance.Culture).ToString();
            }
        }
    }
}
