using System.Drawing;
 

namespace ImageProcessingUtils;

public class MotionDetection
{
    private byte[] _grayBuffer1;
    private byte[] _grayBuffer2;

    //private const int GAUSS_KERNEL_DIMENSION = 10;
    //private const float SIGMA_BLUR = 2f;

    private int _width;
    private int _height;
    private Rectangle _pictureArea;

    public MotionDetection(int width, int height)
    {
        _width = width; _height = height;
        _pictureArea = new Rectangle(0, 0, _width, _height);

        _grayBuffer1 = new byte[width * height];
        _grayBuffer2 = new byte[width * height];
    }

    public void ApplyMotionDetectionFilter(byte[] bytesSource, byte[] bytesDestination, byte[] lastFrame)
    {
        FilterHelper.CropBgra32BitsAndToGray(bytesSource, _pictureArea, _width * 4, _grayBuffer1);
        //SIMDHelper.GaussBlur(_grayBuffer1, _width, _height, SIGMA_BLUR, GAUSS_KERNEL_DIMENSION, _grayBuffer2);
        FilterHelper.CropBgra32BitsAndToGray(lastFrame, _pictureArea, _width * 4, _grayBuffer2);
        FilterHelper.AbsSubstract(_grayBuffer2, _grayBuffer1, _grayBuffer1);
        //FilterHelper.Dilation(_grayBuffer1, _width, _height, _grayBuffer2, 5);
        SIMDHelper.Threshold(_grayBuffer1, _grayBuffer2, _width, _height, 50, 255, 0);
        Buffer.BlockCopy(bytesSource, 0, bytesDestination, 0, bytesSource.Length);

        Rectangle? ptrboundingBox = SIMDHelper.CalculateBoundingBox(_grayBuffer2, _width, _height);
        if (ptrboundingBox is Rectangle boundingBox)
            SIMDHelper.DrawRectangleOnBgra32bits(bytesDestination, _width, _height, _width * 4, 0, 200, 0, boundingBox);
    }
}
