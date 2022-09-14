using AndroidCamClient;
using AndroidCamClient.JpegStream;
using ImageProcessingUtils;
using PhoneCamWithAndroidCam.Threads;
using ProcessingPipelines.ImageProcessingPipeline;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace ProcessingPipelines.PipelineFeeder
{
    public class PipelineFeederPipeline
    {
        private PhoneCamClient _phoneCamClient;

        public ListBuffering<MemoryStream> RawJpegBuffering;
        public ListBuffering<Bitmap> Bitmaps;
        public MultipleBuffering OutputMultipleBuffering;

        public long LastProcessRawJegStreamMsTime { get; set; }
        public long LastProcessRawJegMsTime { get; set; }
        public long LastProcessBitmapMsTime { get; set; }
        public object LastProcessLock { get; set; } = new();

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
                Stopwatch watch = new();
                int framesNbrInASecond = 0;
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    watch.Start();

                    JpegFrame jpegFrame = PhoneCamClient.GetStreamFrame(_rawJpegStream);
                    RawJpegBuffering.AddRawFrame(new MemoryStream(jpegFrame.ToFullBytesImage()));

                    watch.Stop();
                    framesNbrInASecond++;
                    if (watch.ElapsedMilliseconds > 1000)
                    {
                        watch.Reset();
                        lock(LastProcessLock)
                            LastProcessRawJegStreamMsTime = watch.ElapsedMilliseconds;
                        framesNbrInASecond = 0;
                    }
                }
                _rawJpegStream.Close();
            }
        }

        private void ProcessRawJpeg(object? cancellationTokenSourceObj)
        {
            if (cancellationTokenSourceObj is CancellationTokenSource cancellationTokenSource)
            {
                Stopwatch watch = new();
                int framesNbrInASecond = 0;
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    watch.Start();

                    MemoryStream jpegMemoryStream = RawJpegBuffering.GetRawFrame();
                    Bitmaps.AddRawFrame(new Bitmap(jpegMemoryStream));

                    watch.Stop();
                    framesNbrInASecond++;
                    if (watch.ElapsedMilliseconds > 1000)
                    {
                        watch.Reset();
                        lock (LastProcessLock)
                            LastProcessRawJegMsTime = watch.ElapsedMilliseconds;
                        framesNbrInASecond = 0;
                    }
                }
            }
        }

        private void ProcessBitmaps(object? cancellationTokenSourceObj)
        {
            if (cancellationTokenSourceObj is CancellationTokenSource cancellationTokenSource)
            {
                byte[] pixelsBuffer = new byte[320 * 240 * 4];
                Stopwatch watch = new();
                int framesNbrInASecond = 0;
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    watch.Start();

                    Bitmap bmp = Bitmaps.GetRawFrame();
                    BitmapHelper.ToByteArray(bmp, out _, pixelsBuffer);
                    OutputMultipleBuffering.WaitWriteBuffer(pixelsBuffer, bmp);

                    watch.Stop();
                    framesNbrInASecond++;
                    if (watch.ElapsedMilliseconds > 1000)
                    {
                        watch.Reset();
                        lock (LastProcessLock)
                            LastProcessBitmapMsTime = watch.ElapsedMilliseconds;
                        framesNbrInASecond = 0;
                    }
                }
            }
        }

        public void Dispose()
        {
            RawJpegBuffering.Dispose();
            Bitmaps.Dispose();
            OutputMultipleBuffering.Dispose();
        }
    }
}
