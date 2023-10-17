using AndroidCamClient.JpegStream;

namespace AndroidCamClient
{
    public class PhoneCamClient : IDisposable
    {
        private string _phoneUrl;
        private JpegStreamDecoder? _jpegStreamDecoder;

        public string PhoneIp
        { 
            set
            {
                _phoneUrl = $"http://{value}:4747";
            }
        }

        public PhoneCamClient(string phoneIp)
        {
            PhoneIp = phoneIp;
        }

        public void Dispose() => _jpegStreamDecoder?.Dispose();

        /// <summary>
        /// Can take a few time
        /// </summary>
        /// <returns></returns>
        public Task<Stream> LaunchStream()
        {
            _jpegStreamDecoder?.Dispose();
            _jpegStreamDecoder = new();
            return _jpegStreamDecoder.InitMJpegStream(_phoneUrl + "/video?640x480");
        }

        /// <exception cref="IOException"></exception>
        public static JpegFrame GetStreamFrame(Stream mjpegStream)
        {
            return JpegStreamDecoder.ReadOneFrame(mjpegStream);
        }
    }
}