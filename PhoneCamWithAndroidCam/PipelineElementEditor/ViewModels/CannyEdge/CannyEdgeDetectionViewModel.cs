﻿using ImageProcessingUtils.FrameProcessor;
using PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels;
using ProcessingPipelines.ImageProcessingPipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.PipelineElementEditor.ViewModels.CannyEdge
{
    internal class CannyEdgeDetectionViewModel : PipelineElementViewModel
    {
        public int MIN_GAUSS_KERNEL_DIM { get; } = 3;
        public int MAX_GAUSS_KERNEL_DIM { get; } = 20;

        public byte MIN_MIN_GRADIENT_VALUE { get; } = 1;
        public byte MAX_MIN_GRADIENT_VALUE { get; } = 255;

        public double MIN_GRADIENT_MAGNITUDE_SCALE { get; } = 0d;
        public double MAX_GRADIENT_MAGNITUDE_SCALE { get; } = 1d;

        public float MIN_SIGMA_BLUR { get; } = 0.5f;
        public float MAX_SIGMA_BLUR { get; } = 3f;

        private CannyEdgeDetection _cannyEdgeDetection;

        public int GaussKernelDimension
        {
            get => _cannyEdgeDetection.GaussKernelDimension;
            set
            {
                _cannyEdgeDetection.GaussKernelDimension = value;
                NotifyPropertyChanged(nameof(GaussKernelDimension));
            }
        }
        public byte MinGradientValue
        {
            get => _cannyEdgeDetection.MinGradientValue;
            set
            {
                _cannyEdgeDetection.MinGradientValue = value;
                NotifyPropertyChanged(nameof(MinGradientValue));
            }
        }
        public double MinGradientMagnitudeScale
        {
            get => _cannyEdgeDetection.MinGradientMagnitudeScale;
            set 
            {
                _cannyEdgeDetection.MinGradientMagnitudeScale = value;
                NotifyPropertyChanged(nameof(MinGradientMagnitudeScale));
            }
        }
        public double MaxGradientMagnitudeScale
        {
            get => _cannyEdgeDetection.MaxGradientMagnitudeScale;
            set 
            {
                _cannyEdgeDetection.MaxGradientMagnitudeScale = value;
                NotifyPropertyChanged(nameof(MaxGradientMagnitudeScale));
            }
        }
        public float SigmaBlur
        {
            get => _cannyEdgeDetection.SigmaBlur;
            set 
            {
                _cannyEdgeDetection.SigmaBlur = value;
                NotifyPropertyChanged(nameof(SigmaBlur));
            }
        }

        public CannyEdgeDetectionViewModel(string elementName, CannyEdgeDetection cannyEdgeDetection) : base(elementName)
        {
            _cannyEdgeDetection = cannyEdgeDetection;
        }
    }
}
