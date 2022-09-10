using ImageProcessingUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.Threads
{
    internal class PictureBufferingList
    {
        public List<byte[]> BytesBufferList { get; set; }

        public PictureBufferingList(int bufferSize, int bufferNbr)
        {
            BytesBufferList = new List<byte[]>();

            for(int i = 0; i < bufferNbr; i++)
            {
                BytesBufferList.Add(new byte[bufferSize]);
            }
        }

        public byte[] GetBuffer()
        {
            foreach(byte[] buffer in BytesBufferList)
            {
                // try get lock here
                {
                    SIMDHelper.
                }
            }
        }
    }
}
