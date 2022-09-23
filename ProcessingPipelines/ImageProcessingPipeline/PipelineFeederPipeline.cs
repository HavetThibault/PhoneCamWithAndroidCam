using AndroidCamClient;
using AndroidCamClient.JpegStream;
using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;
using System.Diagnostics;
using System.Drawing;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public class PipelineFeederPipeline
    {
        private PhoneCamClient _phoneCamClient;

        public ListBuffering<MemoryStream> RawJpegBuffering;
        public ListBuffering<Bitmap> Bitmaps;
        public MultipleBuffering OutputMultipleBuffering;

        public ProcessPerformances ProcessRawJpegStreamPerf { get; set; }
        public ProcessPerformances ProcessRawJpegPerf { get; set; }
        public ProcessPerformances ProcessBitmapsPerf { get; set; }

        public PipelineFeederPipeline(PhoneCamClient phoneCamClient, MultipleBuffering outputMultipleBuffering)
        {
            _phoneCamClient = phoneCamClient;
            OutputMultipleBuffering = outputMultipleBuffering;
            Bitmaps = new(10);
            RawJpegBuffering = new(10);

            ProcessRawJpegStreamPerf = new("ProcessRawJpegStream");
            ProcessRawJpegPerf = new("ProcessRawJpeg");
            ProcessBitmapsPerf = new("ProcessBitmap");
        }

        public void StartFeeding(CancellationTokenSource cancellationTokenSource)
        {
            var processRawJpegStreamThread = new Thread(ProcessRawJegStream)
            {
                Name = nameof(ProcessRawJegStream)
            };
            processRawJpegStreamThread.Start(cancellationTokenSource);

            var processRawJpegThread = new Thread(ProcessRawJpeg)
            {
                Name = nameof(ProcessRawJpeg)
            };
            processRawJpegThread.Start(cancellationTokenSource);

            var processBitmapsThread = new Thread(ProcessBitmaps)
            {
                Name = nameof(ProcessBitmaps)
            };
            processBitmapsThread.Start(cancellationTokenSource);
        }

        private async void ProcessRawJegStream(object? cancellationTokenSourceObj)
        {
            if (cancellationTokenSourceObj is CancellationTokenSource cancellationTokenSource)
            {
                Stream _rawJpegStream;
                try
                {
                    _rawJpegStream = await _phoneCamClient.LaunchStream();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception de type : {ex.GetType()} : {ex.Message}");
                    cancellationTokenSource.Cancel();
                    return;
                }
                Stopwatch waitingReadTimeWatch = new();
                Stopwatch waitingWriteTimeWatch = new();
                Stopwatch processTimeWatch = new();
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    waitingReadTimeWatch.Start();
                    JpegFrame jpegFrame = PhoneCamClient.GetStreamFrame(_rawJpegStream);
                    waitingReadTimeWatch.Stop();

                    processTimeWatch.Start();
                    var bmpMemoryStream = new MemoryStream(jpegFrame.ToFullBytesImage());
                    processTimeWatch.Stop();

                    waitingWriteTimeWatch.Start();
                    RawJpegBuffering.AddRawFrame(bmpMemoryStream);
                    waitingWriteTimeWatch.Stop();

                    if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                    {
                        lock (ProcessRawJpegStreamPerf)
                        {
                            ProcessRawJpegStreamPerf.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                            ProcessRawJpegStreamPerf.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                            ProcessRawJpegStreamPerf.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                        }
                        waitingReadTimeWatch.Reset();
                        processTimeWatch.Reset();
                        waitingWriteTimeWatch.Reset();
                    }
                }
                _rawJpegStream.Close();
            }
        }

        private void ProcessRawJpeg(object? cancellationTokenSourceObj)
        {
            if (cancellationTokenSourceObj is CancellationTokenSource cancellationTokenSource)
            {
                Stopwatch waitingReadTimeWatch = new();
                Stopwatch waitingWriteTimeWatch = new();
                Stopwatch processTimeWatch = new();
                try
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        waitingReadTimeWatch.Start();
                        MemoryStream jpegMemoryStream = RawJpegBuffering.GetRawFrame();
                        waitingReadTimeWatch.Stop();

                        processTimeWatch.Start();
                        var bmpFrame = new Bitmap(jpegMemoryStream);
                        processTimeWatch.Stop();

                        waitingWriteTimeWatch.Start();
                        Bitmaps.AddRawFrame(bmpFrame);
                        waitingWriteTimeWatch.Stop();

                        if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                        {
                            lock (ProcessRawJpegPerf)
                            {
                                ProcessRawJpegPerf.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                                ProcessRawJpegPerf.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                                ProcessRawJpegPerf.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                            }
                            waitingReadTimeWatch.Reset();
                            processTimeWatch.Reset();
                            waitingWriteTimeWatch.Reset();
                        }
                    }
                }
                catch { } // For 'RawJpegBuffering.GetRawFrame();'
            }
        }

        private void ProcessBitmaps(object? cancellationTokenSourceObj)
        {
            if (cancellationTokenSourceObj is CancellationTokenSource cancellationTokenSource)
            {
                byte[] pixelsBuffer = new byte[OutputMultipleBuffering.Stride * OutputMultipleBuffering.Height];
                Stopwatch waitingReadTimeWatch = new();
                Stopwatch waitingWriteTimeWatch = new();
                Stopwatch processTimeWatch = new();
                try
                {
                    while (!cancellationTokenSource.IsCancellationRequested)
                    {
                        waitingReadTimeWatch.Start();
                        Bitmap bmp = Bitmaps.GetRawFrame();
                        waitingReadTimeWatch.Stop();

                        processTimeWatch.Start();
                        lock(bmp)
                            BitmapHelper.ToByteArray(bmp, out _, pixelsBuffer);
                        processTimeWatch.Stop();

                        waitingWriteTimeWatch.Start();
                        OutputMultipleBuffering.WaitWriteBuffer(pixelsBuffer, bmp);
                        waitingWriteTimeWatch.Stop();

                        if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                        {
                            lock (ProcessBitmapsPerf)
                            {
                                ProcessBitmapsPerf.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                                ProcessBitmapsPerf.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                                ProcessBitmapsPerf.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                            }
                            waitingReadTimeWatch.Reset();
                            processTimeWatch.Reset();
                            waitingWriteTimeWatch.Reset();
                        }
                    }
                }
                catch { } // For Bitmaps.GetRawFrame();
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
