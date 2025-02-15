using Edulink.Communication.Models;
using Edulink.Models;
using Edulink.MVVM;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Edulink.ViewModels
{
    public class DesktopPreviewDialogViewModel : ViewModelBase
    {
        public Client Client { get; private set; }

        private string _title;
        public string Title
        {
            get => _title;
            private set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime TimeStamp;

        private Bitmap _desktopBitmap;

        private BitmapImage _desktopImage;
        public BitmapImage DesktopImage
        {
            get => _desktopImage;
            private set
            {
                if (_desktopImage != value)
                {
                    _desktopImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility _statusTextVisibility = Visibility.Visible;
        public Visibility StatusTextVisibility
        {
            get => _statusTextVisibility;
            private set
            {
                if (_statusTextVisibility != value)
                {
                    _statusTextVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public DesktopPreviewDialogViewModel(Client client)
        {
            Client = client;
            Title = $"{Client.Name} - {Application.Current.TryFindResource("DesktopPreview.ReceivingImage")}";
        }

        public void UpdateScreenshot(Bitmap bitmap)
        {
            TimeStamp = DateTime.Now;
            Title = $"{Client.Name} - {TimeStamp.ToString("dd/MM/yyyy HH:mm:ss")}";
            _desktopBitmap = AddWatermarkToBitmap(bitmap);
            DesktopImage = BitmapToBitmapImage(_desktopBitmap);
            StatusTextVisibility = Visibility.Collapsed;
            OnRequestFocus();
        }

        #region Commands
        public RelayCommand SaveCommand => new RelayCommand(execute => Save());
        private void Save()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = $"{Client.Name}_{TimeStamp:dd-MM-yyyy_HH-mm-ss}",
                DefaultExt = ".png",
                Filter = "PNG files (*.png)|*.png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    _desktopBitmap.Save(stream, ImageFormat.Png);
                }
            }
        }

        public RelayCommand RefreshCommand => new RelayCommand(async execute => await Refresh());
        private async Task Refresh()
        {
            try
            {
                Title = $"{Client.Name} - {Application.Current.TryFindResource("DesktopPreview.ReceivingImage")}";
                await Client.Helper.SendCommandAsync(new EdulinkCommand() { Command = "DESKTOP" });
            }
            catch (Exception)
            {
                OnRequestClose();
            }
        }
        #endregion

        #region Bitmap Helpers
        public BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public Bitmap AddWatermarkToBitmap(Bitmap originalImage)
        {
            int additionalHeight = 52;
            Bitmap watermarkedImage = new Bitmap(originalImage.Width, originalImage.Height + additionalHeight);

            using (Graphics graphics = Graphics.FromImage(watermarkedImage))
            {
                graphics.Clear(Color.White);
                graphics.DrawImage(originalImage, 0, additionalHeight, originalImage.Width, originalImage.Height);

                Bitmap resizedLogo = new Bitmap(LoadBitmapFromResources("Resources/Edulink_32px.png"), new System.Drawing.Size(32, 32));
                graphics.DrawImage(resizedLogo, 10, 10);

                using (Font font = new Font("Arial", 24, System.Drawing.FontStyle.Regular))
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 0, 0, 0)))
                    {
                        graphics.DrawString("Edulink", font, brush, new PointF(46, 8));
                    }
                }
            }

            return watermarkedImage;
        }

        public Bitmap LoadBitmapFromResources(string resourceName)
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri($"pack://application:,,,/{resourceName}"));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(memoryStream);
                return new Bitmap(memoryStream);
            }
        }
        #endregion

        #region Events
        public event EventHandler RequestClose;
        protected virtual void OnRequestClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler RequestFocus;
        protected virtual void OnRequestFocus()
        {
            RequestFocus?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
