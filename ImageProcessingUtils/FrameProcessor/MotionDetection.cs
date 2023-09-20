using System.Drawing;

namespace ImageProcessingUtils.FrameProcessor;

public class MotionDetection : FrameProcessor
{
    public const string ELEMENT_TYPE_NAME = "Motion detection";

    private byte[] _grayBuffer1;
    private byte[] _grayBuffer2;

    private byte[] _lastFrame;

    private Rectangle _pictureArea;

    private MotionDetection() : base() { }

    public MotionDetection(int width, int height) : base(width, height, ELEMENT_TYPE_NAME)
    {
        _pictureArea = new Rectangle(0, 0, _width, _height);
        InitBuffers();
    }

    public MotionDetection(MotionDetection motionDetection) 
        : this(motionDetection._width, motionDetection._height)
    { }

    private void InitBuffers()
    {
        int bufferSize = _width * _height;
        _grayBuffer1 = new byte[bufferSize];
        _grayBuffer2 = new byte[bufferSize];
        _lastFrame = new byte[bufferSize * 4];
    }

    public override void ProcessFrame(byte[] bytesSource, byte[] bytesDestination)
    {
        SIMDHelper.Copy(bytesSource, _lastFrame);

        FilterHelper.CropBgra32BitsAndToGray(bytesSource, _pictureArea, _width * 4, _grayBuffer1);
        FilterHelper.CropBgra32BitsAndToGray(_lastFrame, _pictureArea, _width * 4, _grayBuffer2);
        FilterHelper.AbsSubstract(_grayBuffer2, _grayBuffer1, _grayBuffer1);
        SIMDHelper.Threshold(_grayBuffer1, _grayBuffer2, _width, _height, 50, 255, 0);
        Buffer.BlockCopy(bytesSource, 0, bytesDestination, 0, bytesSource.Length);

        Rectangle? ptrboundingBox = SIMDHelper.CalculateBoundingBox(_grayBuffer2, _width, _height, 16);
        if (ptrboundingBox is Rectangle boundingBox)
            SIMDHelper.DrawRectangleOnBgra32bits(bytesDestination, _width, _height, _width * 4, 0, 200, 0, boundingBox);
    }

    public override FrameProcessor Clone()
    {
        return new MotionDetection(this);
    }

    public override void InitAfterDeserialization()
    {
        _pictureArea = new Rectangle(0, 0, _width, _height);
        InitBuffers();
    }
}
