using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;


namespace WpfUtils
{
    
    namespace Utils
    {
        public static class WpfUtils
        {
            /// <summary>
            /// Convert Bitmap into BitmapImage using a memoryStream
            /// </summary>
            /// <param name="src"></param>
            /// <returns></returns>
            public static BitmapImage Convert(Bitmap src)
            {
                MemoryStream ms = new();
                src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage image = new();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
    }

}
