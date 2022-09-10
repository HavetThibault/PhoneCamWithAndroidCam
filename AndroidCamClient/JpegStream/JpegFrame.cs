using ImageProcessingUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AndroidCamClient.JpegStream
{
    public class JpegFrame
    {
        /// <summary>
        /// Contains jpegStartOfImage + ... + jpegStartOfScan
        /// </summary>
        public byte[] Headers { get; set; }
        /// <summary>
        /// Only contains pixels
        /// </summary>
        public byte[] Scan { get; set; } 

        public JpegFrame(byte[] headers, byte[] scan)
        {
            Headers = headers;
            Scan = scan;
        }

        public byte[] ToFullBytesImage()
        {
            byte[] fullBytesImage = new byte[Headers.Length + Scan.Length + 2];
            SIMDHelper.Copy(Headers, fullBytesImage);
            SIMDHelper.Copy(Scan, fullBytesImage, Headers.Length);
            SIMDHelper.Copy(JpegMarkers.JPEG_END_OF_IMAGE, fullBytesImage, Headers.Length + Scan.Length);
            return fullBytesImage;
        }
    }
}
