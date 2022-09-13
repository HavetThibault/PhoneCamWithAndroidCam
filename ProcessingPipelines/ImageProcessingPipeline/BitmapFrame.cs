using System.Drawing;

namespace ImageProcessingUtils.Pipeline
{
    public class BitmapFrame : Frame
    {
        public Bitmap Bitmap { get; set; }

        public BitmapFrame(byte[] data) : base(data) { Bitmap = null; }

        public BitmapFrame(byte[] data, Bitmap bmp) : base(data)
        {
            Bitmap = bmp;
        }
    }
}
