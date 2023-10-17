using AndroidCamClient;
using AndroidCamClient.JpegStream;
using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Threading;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public class FeederPipeline
    {
        private PhoneCamClient _phoneCamClient;
        private Dispatcher _uiDispatcher;

        public ProducerConsumerBuffers<MemoryStream> RawJpegBuffering;
        public ProducerConsumerBuffers<Bitmap> Bitmaps;
        public ProducerConsumerBuffers OutputMultipleBuffering;

        public ProcessPerformancesModel ProcessRawJpegStreamPerf { get; set; }
        public ProcessPerformancesModel ProcessRawJpegPerf { get; set; }
        public ProcessPerformancesModel ProcessBitmapsPerf { get; set; }

        public FeederPipeline(Dispatcher uiDispatcher, PhoneCamClient phoneCamClient, ProducerConsumerBuffers outputMultipleBuffering)
        {
            _uiDispatcher = uiDispatcher;
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

        /// <summary>
        /// Get a raw jpeg frame from the PhoneCamClient and convert it into a raw byte array JPEG.
        /// Put the result into <see cref="RawJpegBuffering"/>
        /// </summary>
        /// <param name="cancellationTokenSourceObj"></param>
        private void ProcessRawJegStream(object? cancellationTokenSourceObj)
        {
            if (cancellationTokenSourceObj is CancellationTokenSource cancelToken)
            {
                Stopwatch waitingReadTimeWatch = new();
                Stopwatch waitingWriteTimeWatch = new();
                Stopwatch processTimeWatch = new();
                Stream networkRawJpegString = InitJpegStream(cancelToken);
                if (networkRawJpegString is null)
                    return;
                while (!cancelToken.IsCancellationRequested) {
                    waitingReadTimeWatch.Start();
                    JpegFrame jpegFrame;
                    try {
                        jpegFrame = PhoneCamClient.GetStreamFrame(networkRawJpegString);
                    }
                    catch {
                        waitingReadTimeWatch.Stop();
                        networkRawJpegString.Dispose();
                        Thread.Sleep(200);
                        networkRawJpegString = InitJpegStream(cancelToken);
                        if (networkRawJpegString is null)
                            return;
                        continue;
                    }

                    waitingReadTimeWatch.Stop();

                    processTimeWatch.Start();
                    var bmpMemoryStream = new MemoryStream(jpegFrame.GetFullJpeg());
                    processTimeWatch.Stop();

                    waitingWriteTimeWatch.Start();
                    RawJpegBuffering.AddRawFrame(bmpMemoryStream);
                    waitingWriteTimeWatch.Stop();

                    if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                    {
                        try
                        {
                            _uiDispatcher.Invoke(new Action(() =>
                            {
                                ProcessRawJpegStreamPerf.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                                ProcessRawJpegStreamPerf.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                                ProcessRawJpegStreamPerf.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                            }));
                        }
                        catch { break; }
                        waitingReadTimeWatch.Reset();
                        processTimeWatch.Reset();
                        waitingWriteTimeWatch.Reset();
                    }
                }
                networkRawJpegString.Dispose();
            }
        }

        private Stream? InitJpegStream(CancellationTokenSource cancelToken)
        {
            while(!cancelToken.IsCancellationRequested)
            {
                var networkRawJpegStringTask = _phoneCamClient.LaunchStream();

                while (!networkRawJpegStringTask.IsCompleted && !cancelToken.IsCancellationRequested)
                    Thread.Sleep(200);

                if (networkRawJpegStringTask.IsCompletedSuccessfully)
                    return networkRawJpegStringTask.Result;

                try {
                    networkRawJpegStringTask.Result.Dispose();
                }
                catch { }
            }
            return null;
        }

        /// <summary>
        /// <para>Create a Bitmap for each raw JPEG byte from <see cref="RawJpegBuffering"/>.</para>
        /// <para>Put the result into <see cref="Bitmaps"/>.</para>
        /// <para>Bitmap is here used for later manipulation of the JPEG, the JPEG is in a compressed format.</para>
        /// </summary>
        /// <param name="cancellationTokenSourceObj"></param>
        private void ProcessRawJpeg(object? cancellationTokenSourceObj)
        {
            if (cancellationTokenSourceObj is CancellationTokenSource cancellationTokenSource)
            {
                Stopwatch waitingReadTimeWatch = new();
                Stopwatch waitingWriteTimeWatch = new();
                Stopwatch processTimeWatch = new();
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    waitingReadTimeWatch.Start();
                    MemoryStream? jpegMemoryStream = RawJpegBuffering.GetRawFrame();
                    waitingReadTimeWatch.Stop();

                    if (jpegMemoryStream == null)
                        return;

                    processTimeWatch.Start();
                    var bmpFrame = new Bitmap(jpegMemoryStream);
                    processTimeWatch.Stop();

                    waitingWriteTimeWatch.Start();
                    Bitmaps.AddRawFrame(bmpFrame);
                    waitingWriteTimeWatch.Stop();


                    if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                    {
                        try
                        {
                            _uiDispatcher.Invoke(new Action(() =>
                            {
                                ProcessRawJpegPerf.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                                ProcessRawJpegPerf.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                                ProcessRawJpegPerf.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                            }));
                        }
                        catch { break; }
                        waitingReadTimeWatch.Reset();
                        processTimeWatch.Reset();
                        waitingWriteTimeWatch.Reset();
                    }
                }
            }
        }

        /// <summary>
        /// Convert the Bitmaps into an JPEG byte array with uncompressed format.
        /// </summary>
        /// <param name="cancellationTokenSourceObj"></param>
        private void ProcessBitmaps(object? cancellationTokenSourceObj)
        {
            if (cancellationTokenSourceObj is CancellationTokenSource cancellationTokenSource)
            {
                byte[] pixelsBuffer = new byte[OutputMultipleBuffering.Stride * OutputMultipleBuffering.Height];
                Stopwatch waitingReadTimeWatch = new();
                Stopwatch waitingWriteTimeWatch = new();
                Stopwatch processTimeWatch = new();
                int lastSecondProcessedFramesNbr = 0;
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    waitingReadTimeWatch.Start();
                    Bitmap? bmp = Bitmaps.GetRawFrame();
                    waitingReadTimeWatch.Stop();

                    if (bmp == null)
                        return;

                    processTimeWatch.Start();
                    lock(bmp)
                        BitmapHelper.ToByteArray(bmp, out _, pixelsBuffer);
                    processTimeWatch.Stop();

                    waitingWriteTimeWatch.Start();
                    OutputMultipleBuffering.WaitWriteBuffer(pixelsBuffer, bmp);
                    waitingWriteTimeWatch.Stop();
                    lastSecondProcessedFramesNbr++;

                    if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                    {
                        try
                        {
                            _uiDispatcher.Invoke(new Action(() => {
                                ProcessBitmapsPerf.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                                ProcessBitmapsPerf.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                                ProcessBitmapsPerf.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                                ProcessBitmapsPerf.Fps = lastSecondProcessedFramesNbr;
                            }));
                        }
                        catch { break; }
                        lastSecondProcessedFramesNbr = 0;
                        waitingReadTimeWatch.Reset();
                        processTimeWatch.Reset();
                        waitingWriteTimeWatch.Reset();
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

        public void DisposeExceptOutputBuffer()
        {
            RawJpegBuffering.Dispose();
            Bitmaps.Dispose();
        }
    }
}
