using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.FrameProcessor
{
    public class CopyFrameProcessor : IFrameProcessor
    {
        public const string ELEMENT_TYPE_NAME = "Copy";

        public string ElementTypeName { get; set; }

        public CopyFrameProcessor() 
        {
            ElementTypeName = ELEMENT_TYPE_NAME;
        }

        public void ProcessFrame(byte[] srcFrame, byte[] dstFrame)
        {
            Buffer.BlockCopy(srcFrame, 0, dstFrame, 0, dstFrame.Length);
        }

        public IFrameProcessor Clone()
        {
            return new CopyFrameProcessor();
        }
    }
}
