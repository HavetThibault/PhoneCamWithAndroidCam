using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.FrameProcessor
{
    public class GrayColorMapIncrementor : FrameProcessor
    {
        private byte[] _tempGrayBuffer;

        private byte[] _colorMap;
        private int[] _upOrDownColorMapIncrement;

        public GrayColorMapIncrementor(int width, int height, int stride) : 
            base(width, height, stride)
        {
            _colorMap = new byte[256];
            _upOrDownColorMapIncrement = new int[256];
            _tempGrayBuffer = new byte[_width * _height];
            InitColorMapAndCo();
        }

        private void InitColorMapAndCo()
        {
            for (int i = 0; i < _colorMap.Length; i++)
            {
                _colorMap[i] = (byte)i;

                if (i == 255)
                    _upOrDownColorMapIncrement[i] = -1;
                else
                    _upOrDownColorMapIncrement[i] = 1;
            }
        }

        public override void ProcessFrame(byte[] srcFrame, byte[] dstFrame)
        {
            SIMDHelper.BgraToGrayAndChangeColorAndToBgra(srcFrame, _width, _height, _stride, 
                _colorMap, dstFrame, _stride, _tempGrayBuffer);
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
