namespace ProcessingPipelines.PipelineUtils
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
            BytesBuffers = new();

            ElementsNbr = bufferNbr;
            _unReadElementNbr = 0;
            _canRetreive = new(false);
            _canAdd = new(true);
        }

        /// <exception cref="ObjectDisposedException"/>
        public void AddRawFrame(T frameBuffer)
        {
            while (true)
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

                while (!_canAdd.WaitOne(100)) ;
            }
        }

        /// <exception cref="ObjectDisposedException"/>
        public T GetRawFrame()
        {
            while (true)
            {
                lock (_elementsLock)
                {
                    if (_unReadElementNbr > 0)
                    {
                        T oldestBuffer = BytesBuffers.First();
                        BytesBuffers.Remove(oldestBuffer);
                        UnReadBufferNbr--;
                        return oldestBuffer;
                    }
                }

                while (!_canRetreive.WaitOne(100)) ;
            }
        }

        public void Dispose()
        {
            _canAdd?.Close();
            _canRetreive?.Close();
        }
    }
}
