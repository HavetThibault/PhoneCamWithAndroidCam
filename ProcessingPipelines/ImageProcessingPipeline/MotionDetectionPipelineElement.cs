﻿using ImageProcessingUtils.SpecificFrameProcessor;
using ProcessingPipelines.PipelineUtils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingPipelines.ImageProcessingPipeline
{
    internal class MotionDetectionPipelineElement : PipelineElement
    {
        public MotionDetectionPipelineElement(string name, ProducerConsumerBuffers outputMultipleBuffering) : 
            base(name, outputMultipleBuffering)
        {
        }

        public override void Process(ProducerConsumerBuffers inputBuffer, ProducerConsumerBuffers outputBuffer,
            CancellationTokenSource globalCancellationToken, CancellationTokenSource specificCancellationToken, ProcessPerformances processPerf)
        {
            byte[] destBuffer = new byte[inputBuffer.Stride * inputBuffer.Height];
            byte[] lastFrameBuffer = new byte[inputBuffer.Stride * inputBuffer.Height];
            MotionDetection motionDetectionFilter = new(inputBuffer.Width, inputBuffer.Height);
            Stopwatch waitingReadTimeWatch = new();
            Stopwatch waitingWriteTimeWatch = new();
            Stopwatch processTimeWatch = new();
            while (!globalCancellationToken.IsCancellationRequested && !specificCancellationToken.IsCancellationRequested)
            {
                waitingReadTimeWatch.Start();
                BitmapFrame? frame = inputBuffer.WaitNextReaderBuffer();
                waitingReadTimeWatch.Stop();

                if (frame is null)
                    return;

                processTimeWatch.Start();

                motionDetectionFilter.ApplyMotionDetectionFilter(frame.Data, destBuffer, lastFrameBuffer);
                Buffer.BlockCopy(frame.Data, 0, lastFrameBuffer, 0, destBuffer.Length);

                Monitor.Exit(frame);

                processTimeWatch.Stop();
                inputBuffer.FinishReading();

                waitingWriteTimeWatch.Start();
                outputBuffer.WaitWriteBuffer(destBuffer, frame.Bitmap);
                waitingWriteTimeWatch.Stop();

                if (waitingReadTimeWatch.ElapsedMilliseconds + processTimeWatch.ElapsedMilliseconds + waitingWriteTimeWatch.ElapsedMilliseconds > 1000)
                {
                    lock (processPerf)
                    {
                        processPerf.WaitingWriteTimeMs = waitingWriteTimeWatch.ElapsedMilliseconds;
                        processPerf.WaitingReadTimeMs = waitingReadTimeWatch.ElapsedMilliseconds;
                        processPerf.ProcessTimeMs = processTimeWatch.ElapsedMilliseconds;
                    }
                    waitingReadTimeWatch.Reset();
                    processTimeWatch.Reset();
                    waitingWriteTimeWatch.Reset();
                }
            }
        }
    }
}