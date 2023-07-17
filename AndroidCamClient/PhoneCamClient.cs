using AndroidCamClient.JpegStream;

namespace AndroidCamClient
{
    public class PhoneCamClient : IDisposable
    {
        private string _phoneUrl;
        private JpegStreamDecoder _jpegStreamDecoder;

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
            _jpegStreamDecoder = new();
        }

        public void Dispose() => _jpegStreamDecoder.Dispose();

        public Task<Stream> LaunchStream()
        {
            return _jpegStreamDecoder.InitMJpegStream(_phoneUrl + "/video?640x480");
        }

        public static JpegFrame GetStreamFrame(Stream mjpegStream)
        {
            return JpegStreamDecoder.ReadOneFrame(mjpegStream);
        }
    }
}