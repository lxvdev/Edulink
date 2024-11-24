using System.Windows;

namespace Edulink.Attached
{
    public static class IconHelper
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached(
            "Icon",
            typeof(UIElement),
            typeof(IconHelper),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static UIElement GetIcon(DependencyObject d) => (UIElement)d.GetValue(IconProperty);

        public static void SetIcon(DependencyObject d, UIElement value) => d.SetValue(IconProperty, value);
    }
}
