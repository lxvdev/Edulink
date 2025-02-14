using Edulink.MVVM;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Edulink.ViewModels
{
    public class AboutDialogViewModel
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

        public RelayCommand CloseCommand => new RelayCommand(execute => OnRequestClose());

        public RelayCommand GithubCommand => new RelayCommand(execute => Github());
        private void Github()
        {
            Process.Start("https://github.com/lxvdev/Edulink");
        }

        public event EventHandler RequestClose;
        protected virtual void OnRequestClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
