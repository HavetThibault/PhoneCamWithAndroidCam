using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.FrameProcessor
{
    /// <summary>
    /// On bgra 32bits only
    /// </summary>
    public class MedianFilter : FrameProcessor
    {
        protected int _inputBufferStride;

        public MedianFilter(int width, int height, int inputBufferStride) : base(width, height)
        { 
            _inputBufferStride = inputBufferStride;
        }

        /// <summary>
        /// Destroy the srcFrame !!
        /// </summary>
        public override void ProcessFrame(byte[] srcFrame, byte[] dstFrame)
        {
            SIMDHelper.MedianFilter(srcFrame, _width, _height, _inputBufferStride, 4, dstFrame);
        }
    }
}
