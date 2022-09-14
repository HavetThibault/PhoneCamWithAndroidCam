
using BenchmarkDotNet.Attributes;
using ImageProcessingUtils;

namespace FiltersBenchmark;

public class BBytesCopyingComparison
{
    private byte[] _1000SrcBytesBuffer;
    private byte[] _1000DstBytesBuffer;
    private byte[] _10000SrcBytesBuffer;
    private byte[] _10000DstBytesBuffer;
    private byte[] _100000SrcBytesBuffer;
    private byte[] _100000DstBytesBuffer;

    public BBytesCopyingComparison()
    {
        _1000SrcBytesBuffer = new byte[1000];
        _1000DstBytesBuffer = new byte[1000];
        _10000SrcBytesBuffer = new byte[10000];
        _10000DstBytesBuffer = new byte[10000];
        _100000SrcBytesBuffer = new byte[100000];
        _100000DstBytesBuffer = new byte[100000];
    }

    [Benchmark]
    public void BlockCopy1000Bytes()
    {
        Buffer.BlockCopy(_1000SrcBytesBuffer, 0, _1000DstBytesBuffer, 0, _1000SrcBytesBuffer.Length);
    }

    [Benchmark]
    public void BlockCopy10000Bytes()
    {
        Buffer.BlockCopy(_10000SrcBytesBuffer, 0, _10000DstBytesBuffer, 0, _10000SrcBytesBuffer.Length);
    }

    [Benchmark]
    public void BlockCopy100000Bytes()
    {
        Buffer.BlockCopy(_100000SrcBytesBuffer, 0, _100000DstBytesBuffer, 0, _100000SrcBytesBuffer.Length);
    }

    [Benchmark]
    public void SIMDImportedLib1000Bytes()
    {
        SIMDHelper.Copy(_1000SrcBytesBuffer, _1000DstBytesBuffer);
    }

    [Benchmark]
    public void SIMDImportedLib10000Bytes()
    {
        SIMDHelper.Copy(_10000SrcBytesBuffer, _10000DstBytesBuffer);
    }

    [Benchmark]
    public void SIMDImportedLib100000Bytes()
    {
        SIMDHelper.Copy(_100000SrcBytesBuffer, _100000DstBytesBuffer);
    }
}