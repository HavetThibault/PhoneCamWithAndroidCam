using System.Drawing;


namespace ImageProcessingUtils.FrameProcessor;

public class CannyEdgeDetection : FrameProcessor
{
    public const string ELEMENT_TYPE_NAME = "Canny edge detection";
    private const int WEAK_PIXEL = 128; // for treshold and hysteresis

    private byte[] _grayBuffer1;
    private byte[] _grayBuffer2;
    private double[] _magnitudeBuffer;
    private double[] _angleBuffer;
    private short[] _sobelDxBuffer;
    private short[] _sobelDyBuffer;

    private Rectangle _pictureArea;

    public int GaussKernelDimension { get; set; } = 10;
    public byte MinGradientValue { get; set; } = 160;
    public double MinGradientMagnitudeScale { get; set; } = 0.1;
    public double MaxGradientMagnitudeScale { get; set; } = 0.3;
    public float SigmaBlur { get; set; } = 1.5f;

    public CannyEdgeDetection(int width, int height) : base(width, height, ELEMENT_TYPE_NAME)
    {
        _width = width; _height = height;
        _pictureArea = new Rectangle(0, 0, _width, _height);

        _grayBuffer1 = new byte[width * height];
        _grayBuffer2 = new byte[width * height];
        _magnitudeBuffer = new double[width * height];
        _angleBuffer = new double[width * height];
        _sobelDxBuffer = new short[width * height];
        _sobelDyBuffer = new short[width * height];
    }

    public CannyEdgeDetection(CannyEdgeDetection cannyEdgeDetection) 
        : this(cannyEdgeDetection._width, cannyEdgeDetection._height) 
    { }

    public override void ProcessFrame(byte[] bytesSource, byte[] bytesDestination)
    {
        FilterHelper.CropBgra32BitsAndToGray(bytesSource, _pictureArea, _width * 4, _grayBuffer1);
        SIMDHelper.GaussBlur(_grayBuffer1, _width, _height, SigmaBlur, GaussKernelDimension, _grayBuffer2);
        SIMDHelper.SimdSobelGradient(_grayBuffer2, _width, _height, _width, _sobelDxBuffer, _sobelDyBuffer, _magnitudeBuffer, _angleBuffer);
        FilterHelper.FindLocalMaxima(_magnitudeBuffer, _angleBuffer, _width, _height, _grayBuffer1, out byte max);

        if (max < MinGradientValue)
            max = MinGradientValue;

        byte adaptedMax = Convert.ToByte(max * MaxGradientMagnitudeScale);
        byte adaptedMin = Convert.ToByte(max * MinGradientMagnitudeScale);

        FilterHelper.DoubleThreshold(_grayBuffer1, adaptedMin, adaptedMax, _grayBuffer2, WEAK_PIXEL);
        FilterHelper.Hysteresis(_grayBuffer2, _width, _height, _grayBuffer1, WEAK_PIXEL);

        SIMDHelper.SimdGrayToBgra(_grayBuffer1, _width, _height, _width, bytesDestination, _width * 4);
    }

    public override FrameProcessor Clone()
    {
        return new CannyEdgeDetection(this);
    }
}
