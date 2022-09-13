using System.IO;
using System.Windows.Media.Imaging;


namespace WpfUtils
{
    public static class Utils
    {
        /// <summary>
        /// Convert Bitmap into BitmapImage using a memoryStream
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static BitmapImage Convert(MemoryStream pictureStream)
        {
            BitmapImage image = new();
            image.BeginInit();
            pictureStream.Seek(0, SeekOrigin.Begin);
            image.StreamSource = pictureStream;
            image.EndInit();
            return image;
        }
    }
}
