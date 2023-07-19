using System.Drawing;


namespace ImageProcessingUtils.FrameProcessor;

public class CannyEdgeDetection : FrameProcessor
{
    private byte[] _grayBuffer1;
    private byte[] _grayBuffer2;
    private double[] _magnitudeBuffer;
    private double[] _angleBuffer;
    private short[] _sobelDxBuffer;
    private short[] _sobelDyBuffer;

    private const int WEAK_PIXEL = 128; // for treshold and hysteresis
    private const int GAUSS_KERNEL_DIMENSION = 10;
    private const int MIN_GRADIENT_MAGNITUDE = 160;
    private const double MIN_GRADIENT_MAGNITUDE_SCALE = 0.1;
    private const double MAX_GRADIENT_MAGNITUDE_SCALE = 0.3;
    // private const int MIN_CONTRAST = 15;
    private const float SIGMA_BLUR = 1.5f;

    private Rectangle _pictureArea;

    /// <param name="cursorSize"> Must be odd otherwise the filter is not supported</param>
    public CannyEdgeDetection(int width, int height) : base(width, height)
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

    public override void ProcessFrame(byte[] bytesSource, byte[] bytesDestination)
    {
        FilterHelper.CropBgra32BitsAndToGray(bytesSource, _pictureArea, _width * 4, _grayBuffer1);
        SIMDHelper.GaussBlur(_grayBuffer1, _width, _height, SIGMA_BLUR, GAUSS_KERNEL_DIMENSION, _grayBuffer2);
        SIMDHelper.SimdSobelGradient(_grayBuffer2, _width, _height, _width, _sobelDxBuffer, _sobelDyBuffer, _magnitudeBuffer, _angleBuffer);
        FilterHelper.FindLocalMaxima(_magnitudeBuffer, _angleBuffer, _width, _height, _grayBuffer1, out byte max);

        //if (max < MIN_CONTRAST) // if not enough contrast (max gradient magnitude too low), give up !
        //return;

        if (max < MIN_GRADIENT_MAGNITUDE)
            max = MIN_GRADIENT_MAGNITUDE;

        byte adaptedMax = Convert.ToByte(max * MAX_GRADIENT_MAGNITUDE_SCALE);
        byte adaptedMin = Convert.ToByte(max * MIN_GRADIENT_MAGNITUDE_SCALE);

        FilterHelper.DoubleThreshold(_grayBuffer1, adaptedMin, adaptedMax, _grayBuffer2, WEAK_PIXEL);
        FilterHelper.Hysteresis(_grayBuffer2, _width, _height, _grayBuffer1, WEAK_PIXEL);

        SIMDHelper.SimdGrayToBgra(_grayBuffer1, _width, _height, _width, bytesDestination, _width * 4);
    }
}
