using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.FrameProcessor
{
    public class ColorMapThreshold : FrameProcessor
    {
        public const string ELEMENT_TYPE_NAME = "Color map threshold";

        private int _intervalNbr;

        private byte[] _colorMap;

        public int IntervalNbr
        {
            get => _intervalNbr;
            set
            {
                _intervalNbr = value;
                InitColorMapOnIntervalNumber();
            }
        }

        public ColorMapThreshold(int width, int height, int stride, int intervalNbr) : base(width, height, stride, ELEMENT_TYPE_NAME)
        {
            _colorMap = new byte[256];
            _stride = stride;
            _intervalNbr = intervalNbr;
            InitColorMapOnIntervalNumber();
        }

        public ColorMapThreshold(ColorMapThreshold colorMapThreshold) 
            : this(colorMapThreshold._width, colorMapThreshold._height, colorMapThreshold._stride, colorMapThreshold._intervalNbr)
        { }

        private void InitColorMapOnIntervalNumber()
        {
            double intervalLength = 255 / (double)_intervalNbr;
            int intervalBeginningOffset = 0;
            double intervalColor = intervalLength / 2.0;
            lock(ParamLock)
                for (int i = 0; i < _colorMap.Length; i++)
                {
                    _colorMap[i] = (byte)intervalColor;
                    if (i > intervalBeginningOffset + intervalLength)
                    {
                        intervalBeginningOffset = i;
                        intervalColor += intervalLength;
                    }
                }
        }

        public override void ProcessFrame(byte[] srcBuffer, byte[] dstBuffer)
        {
            int lineOffset;
            int byteWidth = 4 * _width;
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < byteWidth; x += 4)
                {
                    lineOffset = y * _stride + x;
                    lock (ParamLock)
                    {
                        dstBuffer[lineOffset] = _colorMap[srcBuffer[lineOffset]];
                        dstBuffer[lineOffset + 1] = _colorMap[srcBuffer[lineOffset + 1]];
                        dstBuffer[lineOffset + 2] = _colorMap[srcBuffer[lineOffset + 2]];
                    }
                    dstBuffer[lineOffset + 3] = 255;
                }
            }
        }

        public override FrameProcessor Clone()
        {
            return new ColorMapThreshold(this);
        }
    }
}
