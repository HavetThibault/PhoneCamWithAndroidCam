
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ImageProcessingUtils;
using System.Drawing.Imaging;
using System.IO;

namespace FiltersBenchmark;

public class BBitmapManipulation
{
    private byte[] _jpegBuffer;
    private MemoryStream _memoryStream;
    private Bitmap _bitmap;
    private byte[] _destinationBuffer;

    public BBitmapManipulation()
    {
        FileStream fs = new("D:\\Programmation\\MotionDetection\\PhoneCamWithAndroidCam\\FiltersBenchmark\\Resources\\JpegBytes.txt", FileMode.Open);
        long bytesNbr = fs.Seek(0, SeekOrigin.End);
        _jpegBuffer = new byte[bytesNbr];
        fs.Seek(0, SeekOrigin.Begin);
        fs.Read(_jpegBuffer, 0, (int)bytesNbr);
        fs.Close();
        _destinationBuffer = new byte[320 * 240 * 4];
    }

    [IterationSetup(Target = "BitmapStreamInitialization")]
    public void IterationSetup_BitmapStreamInitialization()
    {
        _memoryStream = new(_jpegBuffer);
    }

    [Benchmark]
    public void BitmapStreamInitialization()
    {
        _bitmap = new Bitmap(_memoryStream);
    }

    [IterationCleanup(Target = "BitmapStreamInitialization")]
    public void IterationCleanup_BitmapStreamInitialization()
    {
        _bitmap.Dispose();
    }



    [GlobalSetup(Targets = new string[3] { "BitmapExtractPixels", "BitmapBmpSaveToMemoryStream", "BitmapJpegSaveToMemoryStream" })]
    public void GlobalSetup_BitmapExtractPixels_BitmapSaveToMemoryStream()
    {
        _bitmap = new Bitmap(new MemoryStream(_jpegBuffer));
    }

    [Benchmark]
    public void BitmapExtractPixels()
    {
        BitmapHelper.ToByteArray(_bitmap, out _, _destinationBuffer);
    }


    [IterationSetup(Targets = new string[2] { "BitmapBmpSaveToMemoryStream", "BitmapJpegSaveToMemoryStream" })]
    public void IterationSetup_BitmapSaveToMemoryStream()
    {
        _memoryStream = new();
    }

    [Benchmark]
    public void BitmapBmpSaveToMemoryStream()
    {
        _bitmap.Save(_memoryStream, ImageFormat.Bmp);
    }

    [Benchmark]
    public void BitmapJpegSaveToMemoryStream()
    {
        _bitmap.Save(_memoryStream, ImageFormat.Jpeg);
    }

    [IterationCleanup(Targets = new string[2] { "BitmapBmpSaveToMemoryStream", "BitmapJpegSaveToMemoryStream" })]
    public void IterationCleanup_BitmapBmpSaveToMemoryStream()
    {
        _memoryStream.Close();
    }

    [GlobalCleanup(Targets = new string[3] { "BitmapExtractPixels", "BitmapSaveToMemoryStream", "BitmapJpegSaveToMemoryStream" })]
    public void DisposeBitmap()
    {
        _bitmap = new Bitmap(new MemoryStream(_jpegBuffer));
    }
}