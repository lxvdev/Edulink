using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Edulink
{
    /// <summary>
    /// Interaction logic for DesktopPreviewDialog.xaml
    /// </summary>
    public partial class DesktopPreviewDialog : Window
    {
        private DateTime timeStamp;
        private Bitmap bitmap;
        public ClientInfo ClientInfo;
        public DesktopPreviewDialog(Bitmap bitmap, ClientInfo clientInfo)
        {
            InitializeComponent();
            this.timeStamp = DateTime.Now;
            this.bitmap = AddWatermarkToBitmap(bitmap);
            this.ClientInfo = clientInfo;
            Title = clientInfo.Name;
            PreviewImage.Source = ImageToBitmapImage(this.bitmap);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = $"{timeStamp.ToString("yyyy-MM-dd_HH-mm-ss")}_{ClientInfo.Name}";
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG files (*.png)|*.png";

            if (dlg.ShowDialog() == true)
            {
                using (FileStream stream = new FileStream(dlg.FileName, FileMode.Create))
                {
                    bitmap.Save(stream, ImageFormat.Png);
                }
            }
        }

        private Bitmap AddWatermarkToBitmap(Bitmap originalImage)
        {
            int additionalHeight = 52;
            Bitmap watermarkedImage = new Bitmap(originalImage.Width, originalImage.Height + additionalHeight);

            using (Graphics g = Graphics.FromImage(watermarkedImage))
            {
                g.Clear(Color.White);

                g.DrawImage(originalImage, 0, additionalHeight, originalImage.Width, originalImage.Height);

                Bitmap resizedLogo = new Bitmap(LoadBitmapFromResources("Edulink_32px.png"), new System.Drawing.Size(32, 32));
                g.DrawImage(resizedLogo, 10, 10);

                using (Font font = new Font("Arial", 24, System.Drawing.FontStyle.Regular))
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 0, 0, 0)))
                    {
                        g.DrawString("Edulink", font, brush, new PointF(46, 8));
                    }
                }
            }

            return watermarkedImage;
        }

        public void UpdateScreenshot(Bitmap bitmap)
        {
            this.timeStamp = DateTime.Now;
            this.bitmap = AddWatermarkToBitmap(bitmap);
            PreviewImage.Source = ImageToBitmapImage(this.bitmap);
        }

        private BitmapImage ImageToBitmapImage(Bitmap bitmap)
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

        private Bitmap LoadBitmapFromResources(string resourceName)
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

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await ClientInfo.Helper.SendCommandAsync("DesktopPreview");
        }
    }
}
