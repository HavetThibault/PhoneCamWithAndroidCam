using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.WebSockets;
using System.Windows.Threading;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    /// <summary>
    /// Converts the BitmapFrame into a raw JPEG in a byte array 
    /// </summary>
    public class ConvertToRawJpegThread
    {
        private int _height;
        private int _width;
        private Dispatcher _uiDispatcher;

        public ProducerConsumerBuffers InputBuffers { get; set; }
        public ProducerConsumerBuffers<byte[]> OutputBuffers { get; set; }
        public ProcessPerformancesModel ProcessPerformances { get; set; }

        public ConvertToRawJpegThread(Dispatcher uiDispatcher, ProducerConsumerBuffers inputMultipleBuffering, ProducerConsumerBuffers<byte[]> outputMultipleBuffering)
        {
            _uiDispatcher = uiDispatcher;
            OutputBuffers = outputMultipleBuffering;
            InputBuffers = inputMultipleBuffering;
            ProcessPerformances = new("Convert to raw JPEG");
            _height = inputMultipleBuffering.Height;
            _width = inputMultipleBuffering.Width;
        }

        public void LaunchNewWorker(CancellationTokenSource cancellationTokenSource)
        {
            var processThread = new Thread(Process) { Name = nameof(ConvertToRawJpegThread) };
            processThread.Start(cancellationTokenSource);
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
                    waitingReadTimeWatch.Start();
                    BitmapFrame? bitmapFrame = InputBuffers.WaitNextReaderBuffer();
                    waitingReadTimeWatch.Stop();

                    if (bitmapFrame is null)
                        return;

                    processTimeWatch.Start();
 
                    var bmp = bitmapFrame.Bitmap;
                    bitmapFrame.Bitmap = null;
                    try
                    {
                        lock(bmp)
                            BitmapHelper.FromBgraBufferToBitmap(bmp, bitmapFrame.Data, _width, _height);
                    }
                    catch
                    {
                        Monitor.Exit(bitmapFrame);
                        InputBuffers.FinishReading();
                        bmp.Dispose();
                        continue;
                    }

                    MemoryStream displayJpegStream = new();
                    lock(bmp)
                        bmp.Save(displayJpegStream, ImageFormat.Jpeg);

                    Monitor.Exit(bitmapFrame);

                    InputBuffers.FinishReading();
                    bmp.Dispose();
                    processTimeWatch.Stop();

                    waitingWriteTimeWatch.Start();
                    OutputBuffers.AddRawFrame(displayJpegStream.ToArray());
                    waitingWriteTimeWatch.Stop();

                    if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                    {
                        _uiDispatcher.Invoke(new Action(() => {
                            ProcessPerformances.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                            ProcessPerformances.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                            ProcessPerformances.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                        }));
                        waitingReadTimeWatch.Reset();
                        processTimeWatch.Reset();
                        waitingWriteTimeWatch.Reset();
                    }
                }
            }
        }
    }
}
