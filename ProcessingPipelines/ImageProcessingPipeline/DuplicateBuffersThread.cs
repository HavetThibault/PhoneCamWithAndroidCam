using ProcessingPipelines.PipelineUtils;

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

        public MultipleBuffering AddNewOutputBuffer()
        {
            MultipleBuffering copiedInputMultipleBufferin = (MultipleBuffering)InputMultipleBuffering.Clone();
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
                    BitmapFrame bmpFrame = InputMultipleBuffering.WaitNextReaderBuffer();

                    lock (OutputMultipleBuffers)
                    {
                        foreach (MultipleBuffering buffer in OutputMultipleBuffers)
                        {
                            BitmapFrame copiedBmpFrame;
                            lock (bmpFrame)
                                copiedBmpFrame = (BitmapFrame)bmpFrame.Clone();
                            buffer.WriteBuffer(copiedBmpFrame.Data, copiedBmpFrame.Bitmap); // Not synchronizing to not penalize the other streams
                        }
                    }
                    InputMultipleBuffering.FinishReading();
                }
            }
        }
    }
}
