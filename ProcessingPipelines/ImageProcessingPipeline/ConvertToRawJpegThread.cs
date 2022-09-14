using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;
using System.Drawing;
using System.Drawing.Imaging;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public class ConvertToRawJpegThread
    {
        public MultipleBuffering InputMultipleBuffering { get; set; }
        public ListBuffering<byte[]> OutputMultipleBuffering { get; set; }

        public ConvertToRawJpegThread(MultipleBuffering inputMultipleBuffering, ListBuffering<byte[]> outputMultipleBuffering)
        {
            OutputMultipleBuffering = outputMultipleBuffering;
            InputMultipleBuffering = inputMultipleBuffering;
        }

        public void LaunchNewWorker(CancellationTokenSource cancellationTokenSource)
        {
            new Thread(Process).Start(cancellationTokenSource);
        }

        private void Process(object? cancellationTokenSourceObject)
        {
            if (cancellationTokenSourceObject is CancellationTokenSource cancellationTokenSource)
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    Bitmap bmp;
                    MemoryStream cannyStream;
                    BitmapFrame bitmapFrame = InputMultipleBuffering.WaitNextReaderBuffer();
                    lock (bitmapFrame)
                    {
                        bmp = bitmapFrame.Bitmap;
                        bitmapFrame.Bitmap = null;
                        BitmapHelper.FromBgraBufferToBitmap(bmp, bitmapFrame.Data, 320, 240);
                        cannyStream = new();
                        bmp.Save(cannyStream, ImageFormat.Jpeg);
                    }
                    InputMultipleBuffering.FinishReading();
                    bmp.Dispose();

                    OutputMultipleBuffering.AddRawFrame(cannyStream.ToArray());
                }
            }
        }
    }
}
