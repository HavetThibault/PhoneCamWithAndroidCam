using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.FrameProcessor
{
    public class ColorMapIncrementor : FrameProcessor
    {
        private byte[] _colorMap;
        private int[] _upOrDownColorMapIncrement;

        public ColorMapIncrementor(int width, int height, int stride) : base(width, height, stride)
        {
            _colorMap = new byte[256];
            _upOrDownColorMapIncrement = new int[256];
            _stride = stride;
            InitColorMapAndCo();
        }

        private void InitColorMapAndCo()
        {
            for(int i = 0; i < _colorMap.Length; i++)
            {
                _colorMap[i] = (byte)i;

                if(i == 255)
                    _upOrDownColorMapIncrement[i] = -1;
                else
                    _upOrDownColorMapIncrement[i] = 1;
            }
        }

        public override void ProcessFrame(byte[] srcBuffer, byte[] dstBuffer)
        {
            int lineOffset;
            int byteWidth = 4 * _width;
            for (int y = 0; y < _height; y++)
            {
                for(int x = 0; x < byteWidth; x+=4)
                {
                    lineOffset = y * _stride + x;
                    dstBuffer[lineOffset] = _colorMap[srcBuffer[lineOffset]];
                    dstBuffer[lineOffset + 1] = _colorMap[srcBuffer[lineOffset + 1]];
                    dstBuffer[lineOffset + 2] = _colorMap[srcBuffer[lineOffset + 2]];
                    dstBuffer[lineOffset + 3] = 255;
                }
            }
            IncrementColorMap();
        }

        private void IncrementColorMap()
        {
            int result;
            for (int i = 0; i < _colorMap.Length; i++)
            {
                result = _colorMap[i] + _upOrDownColorMapIncrement[i];
                _colorMap[i] = (byte)result;

                if (_colorMap[i] == 255)
                    _upOrDownColorMapIncrement[i] = -1;
                else if (_colorMap[i] == 0)
                    _upOrDownColorMapIncrement[i] = 1;
            }
        }
    }
}
