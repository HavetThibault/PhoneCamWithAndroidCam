using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.WebSockets;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public class ConvertToRawJpegThread
    {
        private int _height;
        private int _width;

        public MultipleBuffering InputMultipleBuffering { get; set; }
        public ListBuffering<byte[]> OutputMultipleBuffering { get; set; }
        public ProcessPerformances ProcessPerformances { get; set; }

        public ConvertToRawJpegThread(MultipleBuffering inputMultipleBuffering, ListBuffering<byte[]> outputMultipleBuffering)
        {
            OutputMultipleBuffering = outputMultipleBuffering;
            InputMultipleBuffering = inputMultipleBuffering;
            ProcessPerformances = new("ConvertToRawJpeg");
            _height = inputMultipleBuffering.Height;
            _width = inputMultipleBuffering.Width;
        }

        public void LaunchNewWorker(CancellationTokenSource cancellationTokenSource)
        {
            new Thread(Process).Start(cancellationTokenSource);
        }

        private void Process(object? cancellationTokenSourceObject)
        {
            if (cancellationTokenSourceObject is CancellationTokenSource cancellationTokenSource)
            {
                Stopwatch waitingReadTimeWatch = new();
                Stopwatch waitingWriteTimeWatch = new();
                Stopwatch processTimeWatch = new();
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    Bitmap bmp;
                    MemoryStream cannyStream;
                    waitingReadTimeWatch.Start();
                    BitmapFrame bitmapFrame = InputMultipleBuffering.WaitNextReaderBuffer();
                    waitingReadTimeWatch.Stop();

                    processTimeWatch.Start();
                    lock (bitmapFrame)
                    {
                        bmp = bitmapFrame.Bitmap;
                        bitmapFrame.Bitmap = null;
                        BitmapHelper.FromBgraBufferToBitmap(bmp, bitmapFrame.Data, _width, _height);
                        cannyStream = new();
                        bmp.Save(cannyStream, ImageFormat.Jpeg);
                    }
                    InputMultipleBuffering.FinishReading();
                    bmp.Dispose();
                    processTimeWatch.Stop();

                    waitingWriteTimeWatch.Start();
                    OutputMultipleBuffering.AddRawFrame(cannyStream.ToArray());
                    waitingWriteTimeWatch.Stop();

                    if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                    {
                        lock (ProcessPerformances)
                        {
                            ProcessPerformances.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                            ProcessPerformances.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                            ProcessPerformances.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                        }
                        waitingReadTimeWatch.Reset();
                        processTimeWatch.Reset();
                        waitingWriteTimeWatch.Reset();
                    }
                }
            }
        }
    }
}
