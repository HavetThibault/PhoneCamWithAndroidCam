using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils.FrameProcessor
{
    public class FrameLagger : FrameProcessor
    {
        public const string ELEMENT_TYPE_NAME = "Frame lagger";

        private byte[] _lastFrame;
        private int _framesCount = 0;
        private bool _firstIteration = true;

        public int LaggedFramesNbr { get; set; }

        public FrameLagger() : base() { }

        public FrameLagger(int width, int height) : base(width, height, ELEMENT_TYPE_NAME) 
        { 
            LaggedFramesNbr = 10;
            _lastFrame = new byte[width * height * 4];
        }

        public FrameLagger(FrameLagger frameLagger) : base(frameLagger._width, frameLagger._height, frameLagger.ElementTypeName)
        {
            LaggedFramesNbr = frameLagger._width;
            _lastFrame = new byte[_width * _height * 4];
        }

        public override FrameProcessor Clone()
        {
            return new FrameLagger(this);
        }

        public override void InitAfterDeserialization()
        {
            _lastFrame = new byte[_width * _height * 4];
        }

        public override void ProcessFrame(byte[] srcFrame, byte[] dstFrame)
        {
            if(_firstIteration)
            {
                Buffer.BlockCopy(srcFrame, 0, _lastFrame, 0, srcFrame.Length);
                _firstIteration = false;
            }
            else if (_framesCount++ >= LaggedFramesNbr)
            {
                Buffer.BlockCopy(srcFrame, 0, _lastFrame, 0, srcFrame.Length);
                _framesCount = 0;
            }
            Buffer.BlockCopy(_lastFrame, 0, dstFrame, 0, _lastFrame.Length);
        }
    }
}
