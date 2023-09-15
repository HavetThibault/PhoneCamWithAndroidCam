using System.Drawing;


namespace ImageProcessingUtils.FrameProcessor;

public enum ScannerStates
{
    Scanning = 0,
    Waiting = 1,
}

public class Scanner : FrameProcessor
{
    public const string ELEMENT_TYPE_NAME = "Scanner";
    
    private Rectangle _lineRectangle;
    private ScannerStates _scannerState = ScannerStates.Scanning;
    private int _scanIndex = 0;

    private byte[] _scannedPixels;

    private int _scanStep;
    private int _scanIntervalMs;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="scanSpeed">Between 1 and 100</param>
    /// <param name="scanIntervalMs"></param>
    public Scanner(int width, int height, int scanStep, int scanIntervalMs) : base(width, height)
    {
        ElementTypeName = ELEMENT_TYPE_NAME;

        _scanStep = scanStep;
        _scanIntervalMs = scanIntervalMs;

        _scannedPixels = new byte[width * height * 4];
        _lineRectangle = new Rectangle(0, 0, 2, height);
    }

    public Scanner(Scanner scanner) 
        : this(scanner._width, scanner._height, scanner._scanStep, scanner._scanIntervalMs) 
    { }

    public override void ProcessFrame(byte[] bytesSource, byte[] bytesDestination)
    {
        switch(_scannerState)
        {
            case ScannerStates.Scanning:
                SIMDHelper.CopyVerticalPart(
                    bytesSource, 
                    _scannedPixels, 
                    _width, 
                    _height, 
                    4, 
                    _scanIndex, 
                    Math.Min(_scanStep, _width - _scanIndex), 
                    _scanIndex);

                if (_scanIndex > 0)
                    SIMDHelper.CopyVerticalPart(
                        _scannedPixels, 
                        bytesDestination, 
                        _width, 
                        _height, 
                        4, 
                        0, 
                        _scanIndex, 
                        0);

                _lineRectangle.X = _scanIndex;
                if(_scanIndex + _lineRectangle.Width <= _width)
                    SIMDHelper.FillBgra(
                        bytesDestination, 
                        _width, 
                        255, 0, 0, 
                        _lineRectangle);

                SIMDHelper.CopyVerticalPart(
                    bytesSource, 
                    bytesDestination, 
                    _width, 
                    _height,
                    4, 
                    _scanIndex + _lineRectangle.Width,
                    _width - _scanIndex - _lineRectangle.Width, 
                    _scanIndex + _lineRectangle.Width);

                _scanIndex += _scanStep;
                if (_scanIndex >= _width)
                {
                    _scannerState = ScannerStates.Waiting;
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(_scanIntervalMs);
                        ResetScan();
                    });
                }
                break;

            case ScannerStates.Waiting:
                Buffer.BlockCopy(_scannedPixels, 0, bytesDestination, 0, _scannedPixels.Length);
                break;
        }
    }

    private void ResetScan()
    {
        _scanIndex = 0;
        _scannerState = ScannerStates.Scanning;
    }

    public override IFrameProcessor Clone()
    {
        return new Scanner(this);
    }
}
