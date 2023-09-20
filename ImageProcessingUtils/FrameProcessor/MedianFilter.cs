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
        public const string ELEMENT_TYPE_NAME = "Median filter";

        private MedianFilter() : base() { }

        public MedianFilter(int width, int height, int inputBufferStride) : base(width, height, inputBufferStride, ELEMENT_TYPE_NAME)
        { }

        public MedianFilter(MedianFilter medianFilter) 
            : this(medianFilter._width, medianFilter._height, medianFilter._stride)
        { }

        /// <summary>
        /// Destroy the srcFrame !!
        /// </summary>
        public override void ProcessFrame(byte[] srcFrame, byte[] dstFrame)
        {
            SIMDHelper.MedianFilter(srcFrame, _width, _height, _stride, 4, dstFrame);
        }

        public override FrameProcessor Clone()
        {
            return new MedianFilter(this);
        }

        public override void InitAfterDeserialization() { }
    }
}
