using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.FrameProcessor
{
    public abstract class FrameProcessor
    {
        protected int _width;
        protected int _height;

        public FrameProcessor(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public abstract void ProcessFrame(byte[] srcFrame, byte[] dstFrame);
    }
}
