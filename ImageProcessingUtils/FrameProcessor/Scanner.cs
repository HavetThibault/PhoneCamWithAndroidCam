﻿using System.Drawing;


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
    public Scanner(int width, int height, int scanStep, int scanIntervalMs) : base(width, height, ELEMENT_TYPE_NAME)
    {
        _scanStep = scanStep;
        _scanIntervalMs = scanIntervalMs;

        _scannedPixels = new byte[width * height * 4];
        _lineRectangle = new Rectangle(0, 0, 2, height);
    }

    public Scanner(Scanner scanner) 
        : this(scanner._width, scanner._height, scanner._scanStep, scanner._scanIntervalMs) 
    { }

    public override void ProcessFrame(byte[] srcBuffer, byte[] destBuffer)
    {
        switch(_scannerState)
        {
            case ScannerStates.Scanning:
                SaveNewSliceIntoScannedPixel(srcBuffer);

                if (_scanIndex > 0)
                    CopyScannedPixelIntoDestBuffer(destBuffer);

                _lineRectangle.X = _scanIndex;
                if (_scanIndex + _lineRectangle.Width <= _width)
                    AddVerticalLine(destBuffer);

                if(_scanIndex + _lineRectangle.Width < _width)
                    CopySourceImageVerticalPartIntoDestBuffer(srcBuffer, destBuffer);

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
            Math.Min(_scanStep, _width - _scanIndex),
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
            _lineRectangle);
    }

    private void CopySourceImageVerticalPartIntoDestBuffer(byte[] srcBuffer, byte[] dstBuffer)
    {
        SIMDHelper.CopyVerticalPart(
            srcBuffer,
            dstBuffer,
            _width,
            _height,
            4,
            _scanIndex + _lineRectangle.Width,
            _width - _scanIndex - _lineRectangle.Width,
            _scanIndex + _lineRectangle.Width);
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
