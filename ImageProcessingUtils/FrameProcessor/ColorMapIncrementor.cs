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
        public const string ELEMENT_TYPE_NAME = "Color map incrementor";

        private int _increment = 1;
        private int _frameCount = 0;
        private byte[] _colorMap;
        private int[] _upOrDownColorMapIncrement;

        public int FramesNbrBeforeIncrement { get; set; } = 1;
        public int Increment 
        {
            get => _increment;
            set
            {
                _increment = value;
                UpdateUpOrDownColorMapOnIncrement();
            }
        }

        public ColorMapIncrementor(int width, int height, int stride) : base(width, height, stride, ELEMENT_TYPE_NAME)
        {
            _colorMap = new byte[256];
            _upOrDownColorMapIncrement = new int[256];
            _stride = stride;
            InitColorMapAndCo();
        }

        public ColorMapIncrementor(ColorMapIncrementor colorMapIncrementor) 
            : this(colorMapIncrementor._width, colorMapIncrementor._height, colorMapIncrementor._stride) 
        { }

        private void InitColorMapAndCo()
        {
            lock(ParamLock)
                for(int i = 0; i < _colorMap.Length; i++)
                {
                    _colorMap[i] = (byte)i;

                    if(i + Increment > 255)
                        _upOrDownColorMapIncrement[i] = -Increment;
                    else
                        _upOrDownColorMapIncrement[i] = Increment;
                }
        }

        private void UpdateUpOrDownColorMapOnIncrement()
        {
            lock(ParamLock)
                for (int i = 0; i < _colorMap.Length; i++)
                {
                    if (_upOrDownColorMapIncrement[i] < 0)
                    {
                        if (_colorMap[i] - Increment >= 0)
                            _upOrDownColorMapIncrement[i] = -Increment;
                        else
                            _upOrDownColorMapIncrement[i] = Increment;
                    }
                    else
                    {
                        if (_colorMap[i] + Increment <= 255)
                            _upOrDownColorMapIncrement[i] = Increment;
                        else
                            _upOrDownColorMapIncrement[i] = -Increment;
                    }
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
                    lock(ParamLock)
                    {
                        dstBuffer[lineOffset] = _colorMap[srcBuffer[lineOffset]];
                        dstBuffer[lineOffset + 1] = _colorMap[srcBuffer[lineOffset + 1]];
                        dstBuffer[lineOffset + 2] = _colorMap[srcBuffer[lineOffset + 2]];
                    }
                    
                    dstBuffer[lineOffset + 3] = 255;
                }
            }
            _frameCount++;
            if(_frameCount >= FramesNbrBeforeIncrement)
            {
                IncrementColorMap();
                _frameCount = 0;
            }
        }

        private void IncrementColorMap()
        {
            int result;
            lock(ParamLock)
                for (int i = 0; i < _colorMap.Length; i++)
                {
                    result = _colorMap[i] + _upOrDownColorMapIncrement[i];
                    _colorMap[i] = (byte)result;

                    if (_colorMap[i] + Increment > 255)
                        _upOrDownColorMapIncrement[i] = -Increment;
                    else if (_colorMap[i] - Increment < 0)
                        _upOrDownColorMapIncrement[i] = Increment;
                }
        }

        public override FrameProcessor Clone()
        {
            return new ColorMapIncrementor(this);
        }
    }
}
