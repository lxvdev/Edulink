using Edulink.Communication;
using Edulink.MVVM;
using System;
using System.Windows.Media.Imaging;

namespace Edulink.Models
{
    public class Client : ViewModelBase, IDisposable
    {
        public Client(TcpHelper helper, string name, string version, bool? updateAvailable)
        {
            Helper = helper;
            Name = name;
            Version = version;
            UpdateAvailable = updateAvailable;
            ConnectionTimestamp = DateTime.Now;
            Endpoint = Helper?.Client.Client.RemoteEndPoint.ToString();
            ID = Guid.NewGuid();
        }

        public TcpHelper Helper { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public bool? UpdateAvailable { get; set; }

        public DateTime ConnectionTimestamp { get; set; }
        public string Endpoint { get; set; }
        public Guid ID { get; set; }

        public TimeSpan ConnectionElapsed => DateTime.Now - ConnectionTimestamp;

        private bool _isExcludedFromPreview;
        public bool IsExcludedFromPreview
        {
            get => _isExcludedFromPreview;
            set
            {
                _isExcludedFromPreview = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage _preview;
        public BitmapImage Preview
        {
            get => _preview;
            set
            {
                _preview = value;
                OnPropertyChanged();
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
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
            get => _isSelected;
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
