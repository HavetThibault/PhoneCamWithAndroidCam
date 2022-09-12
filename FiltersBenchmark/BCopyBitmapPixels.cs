
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ImageProcessingUtils;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FiltersBenchmark;

public class BCopyBitmapPixels
{
    private byte[] _jpegBuffer;
    private Bitmap _bitmap;
    private byte[] _destinationBuffer;

    public BCopyBitmapPixels()
    {
        FileStream fs = new("D:\\Programmation\\MotionDetection\\PhoneCamWithAndroidCam\\FiltersBenchmark\\Resources\\JpegBytes.txt", FileMode.Open);
        long bytesNbr = fs.Seek(0, SeekOrigin.End);
        _jpegBuffer = new byte[bytesNbr];
        fs.Seek(0, SeekOrigin.Begin);
        fs.Read(_jpegBuffer, 0, (int)bytesNbr);
        fs.Close();

        _bitmap = new(new MemoryStream(_jpegBuffer));

        _destinationBuffer = new byte[320 * 240 * 4];
    }
    ~BCopyBitmapPixels()
    {
        _bitmap.Dispose();
    }

    [Benchmark]
    public void BitmapWritePixels()
    {
        BitmapHelper.ToByteArray(_bitmap, out _, _destinationBuffer);
    }
}