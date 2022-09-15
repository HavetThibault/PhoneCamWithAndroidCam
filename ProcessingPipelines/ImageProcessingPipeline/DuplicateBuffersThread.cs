using ImageProcessingUtils;
using ProcessingPipelines.PipelineUtils;
using System.Drawing;
using System.Drawing.Imaging;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public class DuplicateBuffersThread
    {
        public MultipleBuffering InputMultipleBuffering { get; set; }
        public List<MultipleBuffering> OutputMultipleBuffers { get; set; }

        public DuplicateBuffersThread(MultipleBuffering inputMultipleBuffering)
        {
            OutputMultipleBuffers = new();
            InputMultipleBuffering = inputMultipleBuffering;
        }

        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            new Thread(Process).Start(cancellationTokenSource);
        }

        public void AddNewOutputBuffer(MultipleBuffering multipleBuffering)
        {
            lock(OutputMultipleBuffers)
                OutputMultipleBuffers.Add(multipleBuffering);
        }

        private void Process(object? cancellationTokenSourceObject)
        {
            if (cancellationTokenSourceObject is CancellationTokenSource cancellationTokenSource)
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    BitmapFrame bmpFrame = InputMultipleBuffering.GetNextReaderBuffer();
                    BitmapFrame copiedBmpFrame;
                    lock (bmpFrame)
                        copiedBmpFrame = (BitmapFrame)bmpFrame.Clone();

                    lock (OutputMultipleBuffers)
                    {
                        foreach (MultipleBuffering buffer in OutputMultipleBuffers)
                        {
                            BitmapFrame copied2BmpFrame = (BitmapFrame)copiedBmpFrame.Clone();
                            buffer.WriteBuffer(copied2BmpFrame.Data, copied2BmpFrame.Bitmap); // Not synchronizing to not penalize the other streams
                        }
                    }
                }
            }
        }
    }
}
