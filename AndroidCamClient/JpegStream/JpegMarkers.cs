namespace AndroidCamClient.JpegStream
{
    public static class JpegMarkers
    {
        public static readonly byte[] JPEG_START_OF_IMAGE = new byte[2] { 255, 216 };
        public static readonly byte[] JPEG_END_OF_IMAGE = new byte[2] { 255, 217 };
        public static readonly byte[] JPEG_START_OF_SCAN = new byte[2] { 255, 218 };
    }
}
