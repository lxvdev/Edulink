using Edulink.Communication;
using Edulink.MVVM;
using System;
using System.Windows.Media.Imaging;

namespace Edulink.Models
{
    public class Client : ViewModelBase, IDisposable
    {
        private bool _disposed = false;
        public bool Disposed => _disposed;

        public Client(TcpHelper helper, string name, Version version, bool? updateAvailable)
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
        public Version Version { get; set; }
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

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Helper?.Dispose();
                _preview?.StreamSource?.Dispose();
            }

            _disposed = true;
        }

        ~Client()
        {
            Dispose(false);
        }
        #endregion
    }
}
