﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.FrameProcessor
{
    public class Copy : IFrameProcessor
    {
        public Copy() { }

        public void ProcessFrame(byte[] srcFrame, byte[] dstFrame)
        {
            Buffer.BlockCopy(srcFrame, 0, dstFrame, 0, dstFrame.Length);
        }
    }
}
