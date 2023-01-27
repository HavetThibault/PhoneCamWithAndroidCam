namespace AndroidCamClient.JpegStream
{
    /// <summary>
    /// Byte array with Headers and Scan of a JPEG
    /// </summary>
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

        public byte[] GetFullJpeg()
        {
            byte[] fullBytesImage = new byte[Headers.Length + Scan.Length + 2];
            Buffer.BlockCopy(Headers, 0, fullBytesImage, 0, Headers.Length);
            Buffer.BlockCopy(Scan, 0, fullBytesImage, Headers.Length, Scan.Length);
            Buffer.BlockCopy(JpegMarkers.JPEG_END_OF_IMAGE, 0, fullBytesImage, Headers.Length + Scan.Length, JpegMarkers.JPEG_END_OF_IMAGE.Length);
            return fullBytesImage;
        }
    }
}
