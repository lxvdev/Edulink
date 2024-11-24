using System.Windows;

namespace Edulink.Attached
{
    public static class PlaceholderHelper
    {
        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.RegisterAttached(
            "Placeholder",
            typeof(string),
            typeof(PlaceholderHelper),
            new PropertyMetadata(string.Empty));

        public static string GetPlaceholder(DependencyObject d) => (string)d.GetValue(PlaceholderProperty);

        public static void SetPlaceholder(DependencyObject d, string value) => d.SetValue(PlaceholderProperty, value);
    }
}
