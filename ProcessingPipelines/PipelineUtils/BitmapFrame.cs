using System.Drawing;

namespace ProcessingPipelines.PipelineUtils
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
