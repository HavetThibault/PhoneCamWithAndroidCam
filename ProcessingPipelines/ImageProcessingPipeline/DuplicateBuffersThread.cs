using ProcessingPipelines.PipelineUtils;
using System.Drawing;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public class DuplicateBuffersThread
    {
        public ProducerConsumerBuffers InputMultipleBuffering { get; set; }
        public List<ProducerConsumerBuffers> OutputMultipleBuffers { get; set; }

        public DuplicateBuffersThread(ProducerConsumerBuffers inputMultipleBuffering)
        {
            OutputMultipleBuffers = new();
            InputMultipleBuffering = inputMultipleBuffering;
        }

        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            var processThread = new Thread(Process)
            {
                Name = nameof(DuplicateBuffersThread)
            };
            processThread.Start(cancellationTokenSource);
        }

        public ProducerConsumerBuffers AddNewOutputBuffer()
        {
            ProducerConsumerBuffers copiedInputMultipleBufferin = (ProducerConsumerBuffers)InputMultipleBuffering.Clone();
            lock (OutputMultipleBuffers)
                OutputMultipleBuffers.Add(copiedInputMultipleBufferin);
            return copiedInputMultipleBufferin;
        }

        private void Process(object? cancellationTokenSourceObject)
        {
            if (cancellationTokenSourceObject is CancellationTokenSource cancellationTokenSource)
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    BitmapFrame copiedBmpFrame;
                    BitmapFrame? bmpFrame = InputMultipleBuffering.WaitNextReaderBuffer();

                    if (bmpFrame is null)
                        return;

                    copiedBmpFrame = (BitmapFrame)bmpFrame.Clone();
                    Monitor.Exit(bmpFrame);

                    InputMultipleBuffering.FinishReading();

                    lock (OutputMultipleBuffers)
                    {
                        foreach (ProducerConsumerBuffers buffer in OutputMultipleBuffers)
                        {
                            buffer.WriteBuffer(copiedBmpFrame.Data, (Bitmap)copiedBmpFrame.Bitmap.Clone()); // Not synchronizing to not penalize the other streams
                        }
                    }
                    copiedBmpFrame.Bitmap.Dispose();
                }
            }
        }
    }
}
