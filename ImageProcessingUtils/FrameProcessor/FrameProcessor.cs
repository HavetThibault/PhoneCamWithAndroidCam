using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ImageProcessingUtils.FrameProcessor
{
    [XmlInclude(typeof(CannyEdgeDetection))]
    [XmlInclude(typeof(ColorMapIncrementor))]
    [XmlInclude(typeof(ColorMapInverter))]
    [XmlInclude(typeof(ColorMapSaturator))]
    [XmlInclude(typeof(ColorMapThreshold))]
    [XmlInclude(typeof(CopyFrameProcessor))]
    [XmlInclude(typeof(MedianFilter))]
    [XmlInclude(typeof(ScannerProcessor))]
    [XmlInclude(typeof(MotionDetection))]
    public abstract class FrameProcessor
    {
        public int _width { get; set; }
        public int _height { get; set; }
        public int _stride { get; set; }

        public object ParamLock { get; set; } = new ();
        public string ElementTypeName { get; set; }

        protected FrameProcessor() { }

        public FrameProcessor(int width, int height, string elementTypeName)
        {
            ElementTypeName = elementTypeName;
            _width = width;
            _height = height;
            _stride = width;
        }

        public FrameProcessor(int width, int height, int stride, string elementTypeName)
        {
            ElementTypeName = elementTypeName;
            _width = width;
            _height = height;
            _stride = stride;
        }

        public abstract void InitAfterDeserialization();

        public abstract FrameProcessor Clone();

        public abstract void ProcessFrame(byte[] srcFrame, byte[] dstFrame);
    }
}
