using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.Pipeline
{
    public class Frame
    {
        public byte[] Data { get; set; }

        public Frame(byte[] data)
        {
            Data = data;
        }
    }
}
