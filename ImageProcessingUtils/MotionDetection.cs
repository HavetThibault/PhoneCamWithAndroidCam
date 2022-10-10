using System.Drawing;
 

namespace ImageProcessingUtils;

public class MotionDetection
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
    private const int MIN_CONTRAST = 15;
    private const float SIGMA_BLUR = 1.5f;

    private int _width;
    private int _height;
    private Rectangle _pictureArea;

    /// <param name="cursorSize"> Must be odd otherwise the filter is not supported</param>
    public MotionDetection(int width, int height)
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

    public void ApplyMotionDetectionFilter(byte[] bytesSource, byte[] bytesDestination, byte[] lastFrame)
    {
        FilterHelper.CropBgra32BitsAndToGray(bytesSource, _pictureArea, _width * 4, _grayBuffer1);
        SIMDHelper.GaussBlur(_grayBuffer1, _width, _height, SIGMA_BLUR, GAUSS_KERNEL_DIMENSION, _grayBuffer2);
        SIMDHelper.ByteArraysDiff(_grayBuffer2, _width, lastFrame, _width, _width, _height, 1, _grayBuffer1, _width);

        FilterHelper.Threshold(_grayBuffer1, 20, _grayBuffer2);
        FilterHelper.Dilation(_grayBuffer2, _width, _height, _grayBuffer1, 5);

        SIMDHelper.SimdGrayToBgra(_grayBuffer1, _width, _height, _width, bytesDestination, _width * 4);
    }
}
