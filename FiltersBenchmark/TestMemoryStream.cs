
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace FiltersBenchmark;

public class TestMemoryStream
{
    private byte[] _dummyBuffer;
    private MemoryStream _memoryStream;

    public TestMemoryStream()
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
        _memoryStream.Close();
    }
}