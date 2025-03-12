using Edulink.MVVM;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

namespace Edulink.ViewModels
{
    public class AboutDialogViewModel : ClosableViewModel
    {
        public string Product { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Copyright { get; set; }

        public AboutDialogViewModel()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Product = $"{assembly.GetCustomAttribute<AssemblyProductAttribute>().Product}";
            Description = $"{assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description}";
            Version = $"v{assembly.GetName().Version}";
            Copyright = $"{assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright}";
        }

        public ICommand CloseCommand => new RelayCommand(execute => OnRequestClose());

        public ICommand GithubCommand => new RelayCommand(execute => Github());
        private void Github()
        {
            Process.Start("https://github.com/lxvdev/Edulink");
        }
    }
}
