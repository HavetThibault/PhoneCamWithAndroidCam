using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.FrameProcessor
{
    public class CopyFrameProcessor : FrameProcessor
    {
        public const string ELEMENT_TYPE_NAME = "Copy";

        public CopyFrameProcessor() : base(0, 0, ELEMENT_TYPE_NAME)
        {
            ElementTypeName = ELEMENT_TYPE_NAME;
        }

        public override void ProcessFrame(byte[] srcFrame, byte[] dstFrame)
        {
            Buffer.BlockCopy(srcFrame, 0, dstFrame, 0, dstFrame.Length);
        }

        public override FrameProcessor Clone()
        {
            return new CopyFrameProcessor();
        }

        public override void InitAfterDeserialization() { }
    }
}
