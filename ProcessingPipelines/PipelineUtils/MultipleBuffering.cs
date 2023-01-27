using System.Drawing;

namespace ProcessingPipelines.PipelineUtils
{
    public class MultipleBuffering : IDisposable
    {
        private static int idNextNumber = 1;
        private static object idNextNumberLock = new();

        private int _id;
        private int _bufferWriterPointer;
        private int _bufferReaderPointer;
        private int _unReadBufferNbr;
        private readonly AutoResetEvent _canReadBuffer;
        private readonly AutoResetEvent _canWriteBuffer;
        /// <summary>
        /// For <see cref="_unReadBufferNbr"/> - <see cref="_bufferReaderPointer"/> - <see cref="_bufferWriterPointer"/>
        /// </summary>
        private readonly object _bufferPointerLock = new();

        public int BufferNbr { get; init; }
        public BitmapFrame[] BytesBuffers { get; init; }
        public int Height { get; init; }
        public int Width { get; init; }
        public int Stride { get; init; }
        public EBufferPixelsFormat BufferPixelsFormat { get; init; }

        public int UnReadBufferNbr
        {
            get => _unReadBufferNbr;
            set
            {
                _unReadBufferNbr = value;

                if (value == 1)
                    _canReadBuffer.Set();
                else if (value == BufferNbr - 1)
                    _canWriteBuffer.Set();
            }
        }


        public MultipleBuffering(int bufferWidth, int bufferHeight, int bufferNbr, EBufferPixelsFormat bufferPixelsFormat)
            : this(bufferWidth, bufferHeight, bufferWidth, bufferNbr, bufferPixelsFormat) { }

        public MultipleBuffering(int bufferWidth, int bufferHeight, int bufferStride, int bufferNbr, EBufferPixelsFormat bufferPixelsFormat)
        {
            lock (idNextNumberLock)
                _id = idNextNumber++;

            BytesBuffers = new BitmapFrame[bufferNbr];

            Height = bufferHeight;
            Width = bufferWidth;
            Stride = bufferStride;
            BufferPixelsFormat = bufferPixelsFormat;

            for (int i = 0; i < bufferNbr; i++)
            {
                BytesBuffers[i] = new BitmapFrame(new byte[bufferHeight * bufferStride]);
            }

            BufferNbr = bufferNbr;
            _bufferWriterPointer = 0;
            _bufferReaderPointer = 0;
            _unReadBufferNbr = 0;
            _canReadBuffer = new(false);
            _canWriteBuffer = new(true);
        }

        public BitmapFrame GetNextReaderBuffer()
        {
            lock (_bufferPointerLock)
            {
                if (UnReadBufferNbr == 0) return null;

                int bufferReaderPointer = _bufferReaderPointer++;

                if (_bufferReaderPointer == BufferNbr)
                    _bufferReaderPointer = 0;
                return BytesBuffers[bufferReaderPointer];
            }
        }

        public BitmapFrame WaitNextReaderBuffer()
        {
            while (true)
            {
                BitmapFrame nextReaderBuffer = GetNextReaderBuffer();

                if (nextReaderBuffer != null)
                {
                    Monitor.Enter(nextReaderBuffer);
                    return nextReaderBuffer;
                }   

                while (!_canReadBuffer.WaitOne(100)) ;
            }
        }

        public void FinishReading()
        {
            lock (_bufferPointerLock)
                UnReadBufferNbr--;
        }

        public bool WriteBuffer(byte[] newBuffer)
        {
            if (UnReadBufferNbr < BufferNbr)
            {
                int bufferWriterPointer = GetNextWriterPointer();

                BitmapFrame frame = BytesBuffers[bufferWriterPointer];
                lock (frame)
                {
                    Buffer.BlockCopy(newBuffer, 0, frame.Data, 0, newBuffer.Length);
                }

                lock (_bufferPointerLock)
                    UnReadBufferNbr++;

                return true;
            }
            return false;
        }

        public bool WriteBuffer(byte[] newBuffer, Bitmap associatedBitmap)
        {
            if (UnReadBufferNbr < BufferNbr)
            {
                int bufferWriterPointer = GetNextWriterPointer();

                lock (BytesBuffers[bufferWriterPointer])
                {
                    Buffer.BlockCopy(newBuffer, 0, BytesBuffers[bufferWriterPointer].Data, 0, newBuffer.Length);
                    BytesBuffers[bufferWriterPointer].Bitmap = associatedBitmap;
                }

                lock (_bufferPointerLock)
                    UnReadBufferNbr++;

                return true;
            }
            return false;
        }

        private int GetNextWriterPointer()
        {
            int bufferWriterPointer;
            lock (_bufferPointerLock)
            {
                bufferWriterPointer = _bufferWriterPointer++;
                if (_bufferWriterPointer == BufferNbr)
                    _bufferWriterPointer = 0;
            }

            return bufferWriterPointer;
        }

        public void WaitWriteBuffer(byte[] newBuffer, Bitmap associatedBitmap)
        {
            while (true)
            {
                if (WriteBuffer(newBuffer, associatedBitmap))
                    return;

                while (!_canWriteBuffer.WaitOne(100)) ;
            }
        }

        public void WaitWriteBuffer(byte[] newBuffer)
        {
            while (true)
            {
                if (!WriteBuffer(newBuffer))
                    return;

                _canWriteBuffer.WaitOne();
            }
        }

        public object Clone()
        {
            return new MultipleBuffering(Width, Height, Stride, BufferNbr, BufferPixelsFormat);
        }

        public void Dispose()
        {
            _canWriteBuffer?.SafeWaitHandle.SetHandleAsInvalid();
            _canWriteBuffer?.Close();
            _canReadBuffer?.SafeWaitHandle.SetHandleAsInvalid();
            _canReadBuffer?.Close();
        }
    }
}
