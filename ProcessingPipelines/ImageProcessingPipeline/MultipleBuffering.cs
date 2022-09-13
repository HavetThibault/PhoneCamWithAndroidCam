using ImageProcessingUtils;
using ImageProcessingUtils.Pipeline;

namespace PhoneCamWithAndroidCam.Threads
{
    public class MultipleBuffering
    {
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
        public Frame[] BytesBuffers { get; init; }
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
            BytesBuffers = new Frame[bufferNbr];

            Height = bufferHeight;
            Width = bufferWidth;
            Stride = bufferStride;
            BufferPixelsFormat = bufferPixelsFormat;

            for (int i = 0; i < bufferNbr; i++)
            {
                BytesBuffers[i] = new Frame(new byte[bufferHeight * bufferStride]);
            }

            BufferNbr = bufferNbr;
            _bufferWriterPointer = 0;
            _bufferReaderPointer = 0;
            _unReadBufferNbr = 0;
            _canReadBuffer = new(false);
            _canWriteBuffer = new(true);
        }

        public int GetNextReaderBuffer()
        {
            lock (_bufferPointerLock)
            {
                if (UnReadBufferNbr == 0) return -1;
                UnReadBufferNbr--;
                return _bufferReaderPointer++;
            }
        }

        public int WaitNextReaderBuffer()
        {
            while (true)
            {
                int nextReaderBuffer = GetNextReaderBuffer();

                if (nextReaderBuffer != -1)
                    return nextReaderBuffer;

                _canReadBuffer.WaitOne();
            }
        }

        public bool WriteBuffer(byte[] newBuffer)
        {
            if (UnReadBufferNbr < BufferNbr)
            {
                int bufferWriterPointer;
                lock (_bufferPointerLock)
                {
                    bufferWriterPointer = _bufferWriterPointer++;
                    UnReadBufferNbr++;
                }

                //lock (BytesBuffers[bufferWriterPointer])
                SIMDHelper.Copy(newBuffer, BytesBuffers[bufferWriterPointer].Data);

                return true;
            }
            return false;
        }

        public void WaitWriteBuffer(byte[] newBuffer)
        {
            while (true)
            {
                if (!WriteBuffer(newBuffer))
                    _canWriteBuffer.WaitOne();
            }
        }
    }
}
