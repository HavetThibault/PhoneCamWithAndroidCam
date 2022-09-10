using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidCamClient.JpegStream
{
    public class JpegFrame
    {
        public byte[] Headers { get; set; }
        /// <summary>
        /// Does not contain the "start of scan" and "end of image" of jpeg
        /// </summary>
        public byte[] Scan { get; set; } 

        public JpegFrame(byte[] headers, byte[] scan)
        {
            Headers = headers;
            Scan = scan;
        }
    }
}
