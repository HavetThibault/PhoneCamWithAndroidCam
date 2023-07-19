﻿using System;
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
        public MedianFilter(int width, int height, int inputBufferStride) : base(width, height, inputBufferStride)
        { 
        }

        /// <summary>
        /// Destroy the srcFrame !!
        /// </summary>
        public override void ProcessFrame(byte[] srcFrame, byte[] dstFrame)
        {
            SIMDHelper.MedianFilter(srcFrame, _width, _height, _stride, 4, dstFrame);
        }
    }
}