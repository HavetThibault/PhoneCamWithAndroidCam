using ImageProcessingUtils;
using ImageProcessingUtils.Pipeline;

namespace PhoneCamWithAndroidCam.Threads
{
    public class ListBuffering<T> : IDisposable
    {
        private readonly object _elementsLock = new();
        private int _unReadElementNbr;

        private readonly AutoResetEvent _canRetreive;
        private readonly AutoResetEvent _canAdd;

        public int ElementsNbr { get; init; }
        public List<T> BytesBuffers { get; init; }

        public int UnReadBufferNbr
        {
            get => _unReadElementNbr;
            set
            {
                _unReadElementNbr = value;

                if (value == 1)
                    _canRetreive.Set();
                else if (value == ElementsNbr - 1)
                    _canAdd.Set();
            }
        }

        public ListBuffering(int bufferNbr)
        {
            BytesBuffers = new ();

            ElementsNbr = bufferNbr;
            _unReadElementNbr = 0;
            _canRetreive = new(false);
            _canAdd = new(true);
        }

        /// <exception cref="ObjectDisposedException"/>
        public void AddRawFrame(T frameBuffer)
        {
            while(true)
            {
                lock (_elementsLock)
                {
                    if (_unReadElementNbr < ElementsNbr)
                    {
                        BytesBuffers.Add(frameBuffer);
                        UnReadBufferNbr++;
                        return;
                    }
                }

                _canAdd.WaitOne();
            }
        }

        /// <exception cref="ObjectDisposedException"/>
        public T GetRawFrame()
        {
            while(true)
            {
                lock (_elementsLock)
                {
                    if (_unReadElementNbr > 0)
                    {
                        T oldestBuffer = BytesBuffers.First();
                        BytesBuffers.RemoveAt(0);
                        UnReadBufferNbr--;
                        return oldestBuffer;
                    }
                }

                _canRetreive.WaitOne();
            }
        }

        public void Dispose()
        {
            _canAdd.Dispose();
            _canRetreive.Dispose();
        }
    }
}
