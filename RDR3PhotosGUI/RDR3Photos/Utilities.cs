using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RDR3PhotosGUI.RDR3Photos
{
    public static class Utilities
    {
        public static string GetNullTerminatedString(this byte[] data)
        {
            var length = Array.IndexOf(data, byte.MinValue);

            if (length == -1)
                length = data.Length;

            return Encoding.ASCII.GetString(data, 0, length);
        }

        public static ImageSource ToImageSource(this System.Drawing.Image image, ImageFormat imageFormat)
        {
            BitmapImage bitmap = new BitmapImage();

            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, imageFormat);

                stream.Seek(0, SeekOrigin.Begin);

                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }

            return bitmap;
        }
    }
}
