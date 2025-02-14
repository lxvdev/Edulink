using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Media;

namespace Edulink.Models
{
    public class MessageDialog
    {
        public string Message { get; set; }
        public string Title { get; set; }

        public PackIconKind IconKind { get; set; } = PackIconKind.None;
        public SolidColorBrush IconColor { get; set; } = new SolidColorBrush(Colors.Black);
        public Visibility IconVisibility { get; set; } = Visibility.Collapsed;

        public Visibility OkButtonVisibility { get; set; } = Visibility.Collapsed;
        public Visibility ReplyButtonVisibility { get; set; } = Visibility.Collapsed;
        public Visibility CancelButtonVisibility { get; set; } = Visibility.Collapsed;
        public Visibility YesButtonVisibility { get; set; } = Visibility.Collapsed;
        public Visibility NoButtonVisibility { get; set; } = Visibility.Collapsed;

        public MessageDialogButtonResult ButtonResult { get; set; }
        public string ReplyResult { get; set; }
    }
}
