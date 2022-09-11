using ImageProcessingUtils;
using Kyla.Vision.Helpers;
using System;
using System.Drawing;


namespace ImageProcessingUtils;

public class CannyEdgeDetection
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
    public CannyEdgeDetection(int width, int height)
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

    public byte[] ApplyCannyFilter(byte[] bytesSource)
    {
        FilterHelper.CropBgra32BitsAndToGray(bytesSource, _pictureArea, _width * 4, _grayBuffer1);
        SIMDHelper.GaussBlur(_grayBuffer1, _width, _height, SIGMA_BLUR, GAUSS_KERNEL_DIMENSION, _grayBuffer2);
        SIMDHelper.SimdSobelGradient(_grayBuffer2, _width, _height, _width, _sobelDxBuffer, _sobelDyBuffer, _magnitudeBuffer, _angleBuffer);
        FilterHelper.FindLocalMaxima(_magnitudeBuffer, _angleBuffer, _width, _height, _grayBuffer1, out byte max);

        if (max < MIN_CONTRAST) // if not enough contrast (max gradient magnitude too low), give up !
            return null;

        if (max < MIN_GRADIENT_MAGNITUDE)
            max = MIN_GRADIENT_MAGNITUDE;

        byte adaptedMax = Convert.ToByte(max * MAX_GRADIENT_MAGNITUDE_SCALE);
        byte adaptedMin = Convert.ToByte(max * MIN_GRADIENT_MAGNITUDE_SCALE);

        FilterHelper.DoubleThreshold(_grayBuffer1, adaptedMin, adaptedMax, _grayBuffer2, WEAK_PIXEL);
        FilterHelper.Hysteresis(_grayBuffer2, _width, _height, _grayBuffer1, WEAK_PIXEL);

        byte[] resultBuffer = new byte[_width * _height * 4];
        SIMDHelper.SimdGrayToBgra(_grayBuffer1, _width, _height, _width, resultBuffer, _width * 4);
        return resultBuffer;
    }
    /*
    public override Point? FindBestPoint(byte[] sourceVideoframeBuffer)
    {
        FilterHelper.ToGraySquareCrop(sourceVideoframeBuffer, cursorPosition, CursorSize, _grayBuffer1);
        SIMD.GaussBlur(_grayBuffer1, CursorSize, CursorSize, SIGMA_BLUR, GAUSS_KERNEL_DIMENSION, _grayBuffer2);
        SIMD.SimdSobelGradient(_grayBuffer2, CursorSize, CursorSize, CursorSize, _sobelDxBuffer, _sobelDyBuffer, _magnitudeBuffer, _angleBuffer);
        FilterHelper.FindLocalMaxima(_magnitudeBuffer, _angleBuffer, CursorSize, CursorSize, _grayBuffer1, out byte max);

        if (max < MIN_CONTRAST) // if not enough contrast (max gradient magnitude too low), give up !
            return null;

        if (max < MIN_GRADIENT_MAGNITUDE)
            max = MIN_GRADIENT_MAGNITUDE;

        byte adaptedMax = Convert.ToByte(max * MAX_GRADIENT_MAGNITUDE_SCALE);
        byte adaptedMin = Convert.ToByte(max * MIN_GRADIENT_MAGNITUDE_SCALE);

        FilterHelper.DoubleThreshold(_grayBuffer1, adaptedMin, adaptedMax, _grayBuffer2, WEAK_PIXEL);
        FilterHelper.Hysteresis(_grayBuffer2, CursorSize, CursorSize, _grayBuffer1, WEAK_PIXEL);

        if (FilterHelper.BestPointEscargot(_grayBuffer1, CursorSize, CursorSize, true) is Point foundPoint)
            return new Point(foundPoint.X + cursorPosition.X - CursorSize / 2, foundPoint.Y + cursorPosition.Y - CursorSize / 2);
        else
            return null;
    }

    public override Point? FindBestPointAndCopyResult(IVideoFrameBuffer sourceVideoframeBuffer, Point cursorPosition, IEdgeDetectionResultBuffer edgeDetectionResultBuffer)
    {
        Point? resultPoint = FindBestPoint(sourceVideoframeBuffer, cursorPosition);

        if (resultPoint != null)
            lock (edgeDetectionResultBuffer.EdgeDetectionResultBufferLock)
                SIMD.SimdGrayToBgra(_grayBuffer1, CursorSize, CursorSize, CursorSize, edgeDetectionResultBuffer.EdgeDetectionResultBuffer, CursorSize * 4);

        return resultPoint;
    }*/
}
