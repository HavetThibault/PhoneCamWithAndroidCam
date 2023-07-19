using AndroidCamClient;
using ProcessingPipelines.ImageProcessingPipeline;
using ProcessingPipelines.ImageProcessingPipeline.ExperimentalPipelines;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using Helper.MVVM;

namespace PhoneCamWithAndroidCam.ViewModels;

public class DisplayStreamViewModel : BindableClass, IDisposable
{
    private int _fps = 0;
    
    private PhoneCamClient _phoneCamClient;
    private string _phoneIp;

    private bool _isStreaming = false;
    private bool _isPhoneIpChangeable = true;

    private FeederPipeline _feederPipeline;
    private ProducerConsumerBuffers _feederPipelineOutput;

    private Timer _refreshProcessTimer;

    public CancellationTokenSource PipelineCancellationToken { get; private set; }

    public ProcessPerformancesViewModel ProcessPerformancesViewModel { get; set; }

    public int Fps
    {
        get => _fps;
        set => SetProperty(ref _fps, value);
    }

    public string PhoneIp
    {
        get => _phoneIp;
        set
        {
            SetProperty(ref _phoneIp, value);
            _phoneCamClient.PhoneIp = _phoneIp;
        }
    }

    public bool IsPhoneIpChangeable
    {
        get => _isPhoneIpChangeable;
        set => SetProperty(ref _isPhoneIpChangeable, value);
    }

    public bool IsStreaming
    {
        get => _isStreaming;
        set
        {
            SetProperty(ref _isStreaming, value);
            CommandLaunchStreaming.RaiseCanExecuteChanged();
            CommandStopStreaming.RaiseCanExecuteChanged();
        }
    }

    public StreamsViewModel StreamsViewModel { get; set; }

    public RelayCommand CommandLaunchStreaming { get; set; }
    public RelayCommand CommandStopStreaming { get; set; }

    public DisplayStreamViewModel(Dispatcher uiDispatcher, ProcessPerformancesViewModel processPerformancesViewModel)
    {
        CommandLaunchStreaming = new RelayCommand(LaunchStreaming, CanLaunchStreaming);
        CommandStopStreaming = new RelayCommand(StopStreaming, CanStopStreaming);

        _phoneIp = "192.168.1.14";
        _phoneCamClient = new(_phoneIp);

        ProcessPerformancesViewModel = processPerformancesViewModel;

        _feederPipelineOutput = new(640, 480, 640 * 4, 10, EBufferPixelsFormat.Bgra32Bits);
        _feederPipeline = new FeederPipeline(_phoneCamClient, _feederPipelineOutput);
        StreamsViewModel = new(this, uiDispatcher, _feederPipelineOutput);
    }

    public void LaunchStreaming(object parameter)
    {
        IsStreaming = true;
        IsPhoneIpChangeable = false;

        PipelineCancellationToken = new();
        _feederPipeline.StartFeeding(PipelineCancellationToken);
        StreamsViewModel.PlayStreaming(PipelineCancellationToken);
        _refreshProcessTimer = new Timer(RefreshProcessTime, null, 400, 1000);
    }

    public bool CanLaunchStreaming(object parameter)
    {
        return !IsStreaming;
    }

    public void StopStreaming(object parameter)
    {
        IsStreaming = false;
        IsPhoneIpChangeable = true;

        PipelineCancellationToken.Cancel();
        _refreshProcessTimer.Dispose();
        StreamsViewModel.StopStreaming();
    }

    public bool CanStopStreaming(object parameter)
    {
        return IsStreaming;
    }

    public void RefreshProcessTime(object? arg)
    {
        List<ProcessPerformances> perfsList = new()
        {
            _feederPipeline.ProcessRawJpegPerf,
            _feederPipeline.ProcessRawJpegStreamPerf,
            _feederPipeline.ProcessBitmapsPerf
        };

        ProcessPerformancesViewModel.UpdatePerformances(perfsList);
    }

    public void Dispose()
    {
        _feederPipeline?.Dispose();
        _feederPipelineOutput?.Dispose();
        StreamsViewModel.Dispose();
        _phoneCamClient?.Dispose();
    }
}
