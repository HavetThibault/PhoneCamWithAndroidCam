using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.FrameProcessor
{
    public class ColorMapInverter : FrameProcessor
    {
        public const string ELEMENT_TYPE_NAME = "Color map inverter";

        private byte[] _colorMap;

        public ColorMapInverter(int width, int height, int stride) : base(width, height, stride, ELEMENT_TYPE_NAME)
        {
            _colorMap = new byte[256];
            _stride = stride;
            InitColorMapAndCo();
        }

        public ColorMapInverter(ColorMapInverter colorMapInverter) 
            : this(colorMapInverter._width, colorMapInverter._height, colorMapInverter._stride) 
        { }

        private void InitColorMapAndCo()
        {
            for(int i = 0; i < _colorMap.Length; i++)
                _colorMap[i] = (byte)(255 - i);
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
        }

        public override FrameProcessor Clone()
        {
            return new ColorMapInverter(this);
        }
    }
}
