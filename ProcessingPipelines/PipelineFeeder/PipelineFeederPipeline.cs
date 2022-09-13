using AndroidCamClient;
using AndroidCamClient.JpegStream;
using ImageProcessingUtils;
using PhoneCamWithAndroidCam.Threads;
using System.Drawing;

namespace ProcessingPipelines.PipelineFeeder
{
    public class PipelineFeederPipeline
    {
        private PhoneCamClient _phoneCamClient;

        public ListBuffering<MemoryStream> RawJpegBuffering;
        public ListBuffering<Bitmap> Bitmaps;
        public MultipleBuffering OutputMultipleBuffering;

        public PipelineFeederPipeline(PhoneCamClient phoneCamClient, MultipleBuffering outputMultipleBuffering)
        {
            _phoneCamClient = phoneCamClient;
            OutputMultipleBuffering = outputMultipleBuffering;
            Bitmaps = new(10);
            RawJpegBuffering = new (10);
        }

        public void StartFeeding(CancellationTokenSource cancellationTokenSource)
        {
            new Thread(ProcessRawJegStream).Start(cancellationTokenSource);
            new Thread(ProcessRawJpeg).Start(cancellationTokenSource);
            new Thread(ProcessRawJpeg).Start(cancellationTokenSource);
            new Thread(ProcessBitmaps).Start(cancellationTokenSource);
        }

        private async void ProcessRawJegStream(object? cancellationTokenSourceObj)
        {
            if(cancellationTokenSourceObj is CancellationTokenSource cancellationTokenSource)
            {
                Stream _rawJpegStream = await _phoneCamClient.LaunchStream();
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    JpegFrame jpegFrame = PhoneCamClient.GetStreamFrame(_rawJpegStream);
                    RawJpegBuffering.AddRawFrame(new MemoryStream(jpegFrame.ToFullBytesImage()));
                }
                _rawJpegStream.Close();
            }
        }

        private void ProcessRawJpeg(object? cancellationTokenSourceObj)
        {
            if (cancellationTokenSourceObj is CancellationTokenSource cancellationTokenSource)
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    MemoryStream jpegMemoryStream = RawJpegBuffering.GetRawFrame();
                    Bitmaps.AddRawFrame(new Bitmap(jpegMemoryStream));
                }
            }
        }

        private void ProcessBitmaps(object? cancellationTokenSourceObj)
        {
            if (cancellationTokenSourceObj is CancellationTokenSource cancellationTokenSource)
            {
                byte[] pixelsBuffer = new byte[320 * 240 * 4];
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    Bitmap bmp = Bitmaps.GetRawFrame();
                    BitmapHelper.ToByteArray(bmp, out _, pixelsBuffer);
                    bmp.Dispose();
                    OutputMultipleBuffering.WriteBuffer(pixelsBuffer, bmp);
                }
            }
        }
    }
}
