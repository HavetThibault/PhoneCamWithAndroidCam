
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ImageProcessingUtils;
using System.Drawing.Imaging;
using System.IO;

namespace FiltersBenchmark;

public class BUnzipJpeg
{
    private byte[] _jpegBuffer;
    private MemoryStream _memoryStream;
    private Bitmap _bitmap;

    public BUnzipJpeg()
    {
        FileStream fs = new("D:\\Programmation\\MotionDetection\\PhoneCamWithAndroidCam\\FiltersBenchmark\\Resources\\JpegBytes.txt", FileMode.Open);
        long bytesNbr = fs.Seek(0, SeekOrigin.End);
        _jpegBuffer = new byte[bytesNbr];
        fs.Seek(0, SeekOrigin.Begin);
        fs.Read(_jpegBuffer, 0, (int)bytesNbr);
        fs.Close();
    }

    [IterationSetup(Target = "BitmapStreamInitialization")]
    public void NewMemoryStream()
    {
        _memoryStream = new(_jpegBuffer);
    }

    [Benchmark]
    public void BitmapStreamInitialization()
    {
        _bitmap = new Bitmap(_memoryStream);
    }

    [IterationCleanup(Target = "BitmapStreamInitialization")]
    public void CloseMemoryStream()
    {
        _bitmap.Dispose();
    }
}