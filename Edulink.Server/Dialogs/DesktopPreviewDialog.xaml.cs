using System.Windows;
using System.Windows.Media.Imaging;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for DesktopPreviewDialog.xaml
    /// </summary>
    public partial class DesktopPreviewDialog : Window
    {
        public DesktopPreviewDialog(BitmapImage bitmapImage, string name)
        {
            InitializeComponent();
            PreviewImage.Source = bitmapImage;
            Title = name;
        }
    }
}
