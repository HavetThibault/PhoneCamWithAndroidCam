using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.FrameProcessor
{
    public interface IFrameProcessor
    {
        public abstract void ProcessFrame(byte[] srcFrame, byte[] dstFrame);
        public abstract IFrameProcessor Clone();
    }
}
