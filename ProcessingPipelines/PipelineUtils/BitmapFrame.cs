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

        public object Clone()
        {
            byte[] data = new byte[Data.Length];
            Buffer.BlockCopy(Data, 0, data, 0, Data.Length);
            return new BitmapFrame(data) { Bitmap = (Bitmap)Bitmap.Clone() };
        }
    }
}
