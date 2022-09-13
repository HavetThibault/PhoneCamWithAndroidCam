using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
