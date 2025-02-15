using Edulink.Server.MVVM;
using Edulink.Server.Communication;
using System;
using System.Windows.Media.Imaging;

namespace Edulink.Server.Models
{
    public class Client : ViewModelBase, IDisposable
    {
        public Client(TcpHelper helper, string name, string version)
        {
            Helper = helper;
            Name = name;
            Version = version;
            ConnectionTime = DateTime.Now;
            Endpoint = Helper?.Client.Client.RemoteEndPoint.ToString();
        }

        public TcpHelper Helper { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public DateTime ConnectionTime { get; set; }
        public string Endpoint { get; set; }

        private bool _isExcludedFromPreview;

        public bool IsExcludedFromPreview
        {
            get { return _isExcludedFromPreview; }
            set
            {
                _isExcludedFromPreview = value;
                DesktopPreview = null;
                OnPropertyChanged();
            }
        }


        private BitmapImage _desktopPreview;
        public BitmapImage DesktopPreview
        {
            get { return _desktopPreview; }
            set
            {
                _desktopPreview = value;
                OnPropertyChanged();
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged();

                    if (!_isVisible)
                    {
                        IsSelected = false;
                        OnPropertyChanged(nameof(IsSelected));
                    }
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Dispose()
        {
            Helper?.Dispose();
        }
    }
}
