using AndroidCamClient;
using PhoneCamWithAndroidCam.Threads;
using System.Drawing;

namespace ProcessingPipelines.PipelineFeeder
{
    public class PipelineFeederPipeline
    {
        private PhoneCamClient _phoneCamClient;

        private ListBuffering<byte[]> _rawJpegBuffering;
        private ListBuffering<Bitmap> _bitmaps;
        private MultipleBuffering _outputMultipleBuffering;

        private object _bitmapsLock = new object();

        public PipelineFeederPipeline(PhoneCamClient phoneCamClient, MultipleBuffering outputMultipleBuffering)
        {
            _phoneCamClient = phoneCamClient;
            _outputMultipleBuffering = outputMultipleBuffering;
            _bitmaps = new(10);
            _rawJpegBuffering = new (10);
        }

        public void StartFeeding()
        {

        }

        private void ProcessPhoneCamClientStream()
        {

        }

        private void ProcessRawJpeg()
        {

        }

        private void ProcessBitmaps()
        {

        }
    }
}
