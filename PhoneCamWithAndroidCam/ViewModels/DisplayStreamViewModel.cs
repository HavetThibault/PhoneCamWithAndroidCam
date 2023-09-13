﻿using AndroidCamClient;
using ProcessingPipelines.ImageProcessingPipeline;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using Helper.MVVM;
using System.Collections.ObjectModel;

namespace PhoneCamWithAndroidCam.ViewModels;

public class DisplayStreamViewModel : BindableClass, IDisposable
{
    private int _fps = 0;
    private Dispatcher _uiDispatcher;

    private PhoneCamClient _phoneCamClient;
    private string _phoneIp;

    private bool _isStreaming = false;
    private bool _isPhoneIpChangeable = true;

    private FeederPipeline _feederPipeline;
    private ProducerConsumerBuffers _feederPipelineOutput;

    public CancellationTokenSource PipelineCancellationToken { get; private set; }

    public ProcessPerformancesViewModel ProcessPerformancesViewModel { get; set; }

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

    public DisplayStreamViewModel(Dispatcher uiDispatcher)
    {
        CommandLaunchStreaming = new RelayCommand(LaunchStreaming, CanLaunchStreaming);
        CommandStopStreaming = new RelayCommand(StopStreaming, CanStopStreaming);

        _uiDispatcher = uiDispatcher;

        _phoneIp = "192.168.115.245";
        _phoneCamClient = new(_phoneIp);

        ProcessPerformancesViewModel = new();

        _feederPipelineOutput = new(640, 480, 640 * 4, 10, EBufferPixelsFormat.Bgra32Bits);
        _feederPipeline = new FeederPipeline(uiDispatcher, _phoneCamClient, _feederPipelineOutput);

        ProcessPerformancesViewModel.ProcessPerformances.Add(_feederPipeline.ProcessRawJpegStreamPerf);
        ProcessPerformancesViewModel.ProcessPerformances.Add(_feederPipeline.ProcessRawJpegPerf);
        ProcessPerformancesViewModel.ProcessPerformances.Add(_feederPipeline.ProcessBitmapsPerf);

        StreamsViewModel = new(this, uiDispatcher, _feederPipelineOutput);
    }

    public void LaunchStreaming(object parameter)
    {
        IsStreaming = true;
        IsPhoneIpChangeable = false;

        PipelineCancellationToken = new();
        _feederPipeline.StartFeeding(PipelineCancellationToken);
        StreamsViewModel.PlayStreaming(PipelineCancellationToken);
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
    }

    public bool CanStopStreaming(object parameter)
    {
        return IsStreaming;
    }

    public void Dispose()
    {
        _feederPipeline?.Dispose();
        _feederPipelineOutput?.Dispose();
        StreamsViewModel.Dispose();
        _phoneCamClient?.Dispose();
    }
}
