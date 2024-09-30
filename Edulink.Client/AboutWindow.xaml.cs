using System.Reflection;
using System.Windows;
namespace Edulink
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Product.Text = $"{assembly.GetCustomAttribute<AssemblyProductAttribute>().Product}";
            Description.Text = $"{assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description}";
            Version.Text = $"v{assembly.GetName().Version}";
            Copyright.Text = $"{assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright}";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
