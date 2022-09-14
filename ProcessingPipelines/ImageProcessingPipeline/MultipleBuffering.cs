using ImageProcessingUtils;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    public class MultipleBuffering : IDisposable
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
                    return nextReaderBuffer;

                _canReadBuffer.WaitOne();
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
                int bufferWriterPointer;
                lock (_bufferPointerLock)
                {
                    bufferWriterPointer = _bufferWriterPointer++;
                    if (_bufferWriterPointer == BufferNbr)
                        _bufferWriterPointer = 0;
                }

                BitmapFrame frame = BytesBuffers[bufferWriterPointer];
                lock(frame)
                {
                    SIMDHelper.Copy(newBuffer, frame.Data);
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
                int bufferWriterPointer;
                lock (_bufferPointerLock)
                {
                    bufferWriterPointer = _bufferWriterPointer++;
                    if (_bufferWriterPointer == BufferNbr)
                        _bufferWriterPointer = 0;
                } 

                lock (BytesBuffers[bufferWriterPointer])
                {
                    SIMDHelper.Copy(newBuffer, BytesBuffers[bufferWriterPointer].Data);
                    BytesBuffers[bufferWriterPointer].Bitmap = associatedBitmap;
                }

                lock (_bufferPointerLock)
                    UnReadBufferNbr++;

                return true;
            }
            return false;
        }

        public void WaitWriteBuffer(byte[] newBuffer, Bitmap associatedBitmap)
        {
            while (true)
            {
                if (WriteBuffer(newBuffer, associatedBitmap))
                    return;

                _canWriteBuffer.WaitOne();
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

        public void Dispose()
        {
            _canWriteBuffer.Dispose();
            _canReadBuffer.Dispose();
        }
    }
}
