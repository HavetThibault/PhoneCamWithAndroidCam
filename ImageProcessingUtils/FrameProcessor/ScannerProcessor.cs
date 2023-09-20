using System.Drawing;


namespace ImageProcessingUtils.FrameProcessor;

public enum ScannerStates
{
    Scanning = 0,
    Waiting = 1,
}

public class ScannerProcessor : FrameProcessor
{
    public const string ELEMENT_TYPE_NAME = "Scanner";
    
    private Rectangle _scannerLine;
    private ScannerStates _scannerState = ScannerStates.Scanning;
    private int _scanIndex = 0;
    private int _scanStep;

    private byte[] _scannedPixels;

    public int ScanStep
    {
        get => _scanStep;
        set
        {
            lock (ParamLock)
                _scanStep = value;
        }
    }

    public int ScanIntervalMs { get; set; }

    public ScannerProcessor(int width, int height, int scanStep, int scanIntervalMs) : base(width, height, ELEMENT_TYPE_NAME)
    {
        ScanStep = scanStep;
        ScanIntervalMs = scanIntervalMs;

        _scannedPixels = new byte[width * height * 4];
        _scannerLine = new Rectangle(0, 0, 2, height);
    }

    public ScannerProcessor(ScannerProcessor scanner) 
        : this(scanner._width, scanner._height, scanner.ScanStep, scanner.ScanIntervalMs) 
    { }

    public override void ProcessFrame(byte[] srcBuffer, byte[] destBuffer)
    {
        switch(_scannerState)
        {
            case ScannerStates.Scanning:
                lock(ParamLock)
                {
                    SaveNewSliceIntoScannedPixel(srcBuffer);

                    if (_scanIndex > 0)
                        CopyScannedPixelIntoDestBuffer(destBuffer);

                    if (_scanIndex == _width)
                    {
                        _scanIndex = _width;
                        _scannerState = ScannerStates.Waiting;
                        Task.Factory.StartNew(() =>
                        {
                            Thread.Sleep(ScanIntervalMs);
                            ResetScan();
                        });
                        return;
                    }

                    _scannerLine.X = _scanIndex;
                    if (_scanIndex + _scannerLine.Width <= _width)
                        AddVerticalLine(destBuffer);

                    if (_scanIndex + _scannerLine.Width < _width)
                        CopySourceImageVerticalPartIntoDestBuffer(srcBuffer, destBuffer);

                    _scanIndex += ScanStep;
                    if (_scanIndex >= _width)
                        _scanIndex = _width;
                    break;
                }

            case ScannerStates.Waiting:
                Buffer.BlockCopy(_scannedPixels, 0, destBuffer, 0, _scannedPixels.Length);
                break;
        }
    }

    private void SaveNewSliceIntoScannedPixel(byte[] srcBuffer)
    {
        SIMDHelper.CopyVerticalPart(
            srcBuffer,
            _scannedPixels,
            _width,
            _height,
            4,
            _scanIndex,
            Math.Min(ScanStep, _width - _scanIndex),
            _scanIndex);
    }

    private void CopyScannedPixelIntoDestBuffer(byte[] destBuffer)
    {
        SIMDHelper.CopyVerticalPart(
            _scannedPixels,
            destBuffer,
            _width,
            _height,
            4,
            0,
            _scanIndex,
            0);
    }

    private void AddVerticalLine(byte[] destBuffer)
    {
        SIMDHelper.FillBgra(
            destBuffer,
            _width,
            255, 0, 0,
            _scannerLine);
    }

    private void CopySourceImageVerticalPartIntoDestBuffer(byte[] srcBuffer, byte[] dstBuffer)
    {
        SIMDHelper.CopyVerticalPart(
            srcBuffer,
            dstBuffer,
            _width,
            _height,
            4,
            _scanIndex + _scannerLine.Width,
            _width - _scanIndex - _scannerLine.Width,
            _scanIndex + _scannerLine.Width);
    }

    private void ResetScan()
    {
        _scanIndex = 0;
        _scannerState = ScannerStates.Scanning;
    }

    public override IFrameProcessor Clone()
    {
        return new ScannerProcessor(this);
    }
}
