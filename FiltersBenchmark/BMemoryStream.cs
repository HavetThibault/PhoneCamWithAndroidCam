
using BenchmarkDotNet.Attributes;

namespace FiltersBenchmark;

public class BMemoryStream
{
    private byte[] _dummyBuffer;
    private MemoryStream _memoryStream;

    public BMemoryStream()
    {
        _dummyBuffer = new byte[7000];
    }

    [IterationSetup]
    public void NewMemoryStream()
    {
        _memoryStream = new();
    }

    [Benchmark]
    public byte[] MemoryStreamWrite()
    {
        _memoryStream.Write(_dummyBuffer);
        return _dummyBuffer;
    }

    [IterationCleanup]
    public void CloseMemoryStream()
    {
        _memoryStream.Dispose();
    }
}