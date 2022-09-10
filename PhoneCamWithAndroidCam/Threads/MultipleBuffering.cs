using ImageProcessingUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.Threads
{
    internal class MultipleBuffering
    {
        private int _bufferWriterPointer;
        private int _bufferReaderPointer;
        private int _unReadBufferNbr;
        /// <summary>
        /// For <see cref="_unReadBufferNbr"/> and <see cref="_bufferReaderPointer"/>
        /// </summary>
        private object _bufferPointerLock = new();

        public byte[][] BytesBuffers { get; set; }

        public MultipleBuffering(int bufferSize, int bufferNbr)
        {
            BytesBuffers = new byte[bufferNbr][];

            for(int i = 0; i < bufferNbr; i++)
            {
                BytesBuffers[i] = new byte[bufferSize];
            }

            _bufferWriterPointer = 0;
            _bufferReaderPointer = 0;
            _unReadBufferNbr = 0;
        }

        public int GetNextReaderBuffer()
        {
            lock(_bufferPointerLock)
            {
                if (_unReadBufferNbr == 0)
                {
                    return -1;
                }

                _unReadBufferNbr--;
                return _bufferReaderPointer++;
            }
        }

        public int WriteBuffer(byte[] newBuffer)
        {
            if (_unReadBufferNbr < BytesBuffers.Length)
            {
                lock (BytesBuffers[_bufferWriterPointer])
                {
                    SIMDHelper.Copy(newBuffer, BytesBuffers[_bufferWriterPointer]);
                    _bufferWriterPointer++;

                    lock(_bufferPointerLock)
                        _unReadBufferNbr++;
                    return 0;
                }
            }
            return -1;
        }
    }
}
