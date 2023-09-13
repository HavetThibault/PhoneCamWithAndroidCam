using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.FrameProcessor
{
    public abstract class FrameProcessor : IFrameProcessor
    {
        protected int _width;
        protected int _height;
        protected int _stride;

        public string ElementTypeName { get; set; }

        public FrameProcessor(int width, int height)
        {
            _width = width;
            _height = height;
            _stride = width;
        }

        public FrameProcessor(int width, int height, int stride)
        {
            _width = width;
            _height = height;
            _stride = stride;
        }

        public abstract IFrameProcessor Clone();

        public abstract void ProcessFrame(byte[] srcFrame, byte[] dstFrame);
    }
}
