﻿
using BenchmarkDotNet.Attributes;
using ImageProcessingUtils;
using System.Drawing.Imaging;

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
        fs.Dispose();
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



    [GlobalSetup(Targets = new string[4] { "BitmapExtractPixels", "BitmapBmpSaveToMemoryStream", "BitmapJpegSaveToMemoryStream", "CloneBitmap" })]
    public void GlobalSetup_BitmapExtractPixels_BitmapSaveToMemoryStream()
    {
        _bitmap = new Bitmap(new MemoryStream(_jpegBuffer));
    }

    [Benchmark]
    public void CloneBitmap()
    {
        _bitmap.Clone();
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
        _memoryStream.Dispose();
    }

    [GlobalCleanup(Targets = new string[4] { "BitmapExtractPixels", "BitmapSaveToMemoryStream", "BitmapJpegSaveToMemoryStream", "CloneBitmap" })]
    public void DisposeBitmap()
    {
        _bitmap.Dispose();
    }
}