using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ImageProcessingUtils
{
    public static class SIMDHelper
    {
        public static void Threshold(byte[] src, byte[] dest, int width, int height, byte thresholdValue, byte positive, byte negative)
        {
            GCHandle pinnedSrcBuffer = GCHandle.Alloc(src, GCHandleType.Pinned);
            IntPtr srcBufferPtr = pinnedSrcBuffer.AddrOfPinnedObject();

            GCHandle pinnedDestBuffer = GCHandle.Alloc(dest, GCHandleType.Pinned);
            IntPtr destBuffer = pinnedDestBuffer.AddrOfPinnedObject();

            SIMD.SimdBinarization(srcBufferPtr, width, width, height, thresholdValue, positive, negative, destBuffer, width, (int)ESimdCompareType.SimdCompareGreater);

            pinnedDestBuffer.Free();
            pinnedSrcBuffer.Free();
        }

        public static void Copy(byte[] sourceBuffer, byte[] destinationBuffer)
        {
            if (sourceBuffer.Length > destinationBuffer.Length)
                throw new ArgumentException("The destination byte arrays must be long enough.");

            GCHandle pinnedSrcBuffer = GCHandle.Alloc(sourceBuffer, GCHandleType.Pinned);
            IntPtr srcBufferPtr = pinnedSrcBuffer.AddrOfPinnedObject();

            GCHandle pinnedDestBuffer = GCHandle.Alloc(destinationBuffer, GCHandleType.Pinned);
            IntPtr destBuffer = pinnedDestBuffer.AddrOfPinnedObject();

            SIMD.SimdCopy(srcBufferPtr, sourceBuffer.Length, sourceBuffer.Length, 1, 1, destBuffer, sourceBuffer.Length);

            pinnedDestBuffer.Free();
            pinnedSrcBuffer.Free();
        }

        public static void Copy(byte[] sourceBuffer, byte[] destinationBuffer, int dstOffset)
        {
            GCHandle pinnedSrcBuffer = GCHandle.Alloc(sourceBuffer, GCHandleType.Pinned);
            IntPtr srcBufferPtr = pinnedSrcBuffer.AddrOfPinnedObject();

            GCHandle pinnedDestBuffer = GCHandle.Alloc(destinationBuffer, GCHandleType.Pinned);
            IntPtr destBuffer = pinnedDestBuffer.AddrOfPinnedObject();
            IntPtr destBufferMoreOffset = IntPtr.Add(destBuffer, dstOffset);

            SIMD.SimdCopy(srcBufferPtr, sourceBuffer.Length, sourceBuffer.Length, 1, 1, destBufferMoreOffset, sourceBuffer.Length);

            pinnedDestBuffer.Free();
            pinnedSrcBuffer.Free();
        }

        public static void SimdGrayToBgra(byte[] grayBuffer, int width, int height, int stride, byte[] bgrBuffer, int bgrStride)
        {
            GCHandle pinnedSrcBuffer = GCHandle.Alloc(grayBuffer, GCHandleType.Pinned);
            IntPtr srcBufferPtr = pinnedSrcBuffer.AddrOfPinnedObject();

            GCHandle pinnedDestBuffer = GCHandle.Alloc(bgrBuffer, GCHandleType.Pinned);
            IntPtr destBuffer = pinnedDestBuffer.AddrOfPinnedObject();

            SIMD.SimdGrayToBgra(srcBufferPtr, width, height, stride, destBuffer, bgrStride);

            pinnedDestBuffer.Free();
            pinnedSrcBuffer.Free();
        }

        public static byte[] SimdBgraToGray(IntPtr bgra, int width, int height, int bgraStride)
        {
            byte[] grayPixels = new byte[width * height];
            GCHandle grayPinnedArray = GCHandle.Alloc(grayPixels, GCHandleType.Pinned);
            IntPtr _8bitGrayPixels = grayPinnedArray.AddrOfPinnedObject();

            SIMD.SimdBgraToGray(bgra, width, height, bgraStride, _8bitGrayPixels, width);
            grayPinnedArray.Free();

            return grayPixels;
        }

        public static void SimdSobelGradient(byte[] pixelBuffer, int width, int height, int stride, short[] sobelDxPixels, short[] sobelDyPixels, double[] magnitudeBuffer, double[] angleBuffer)
        {
            SobelDx(pixelBuffer, width, height, stride, sobelDxPixels);
            SobelDy(pixelBuffer, width, height, stride, sobelDyPixels);

            FillUnManagedBorder(sobelDxPixels, width, height, 0, 1);
            FillUnManagedBorder(sobelDyPixels, width, height, 0, 1);

            for (int i = 0; i < pixelBuffer.Length; i++)
            {
                magnitudeBuffer[i] = Math.Sqrt(sobelDxPixels[i] * sobelDxPixels[i] + sobelDyPixels[i] * sobelDyPixels[i]);

                angleBuffer[i] = Math.Atan2(sobelDxPixels[i], sobelDyPixels[i]) * (180 / Math.PI);
            }
        }

        /// <summary>
        /// 'src' must be non-strided and 8-bit gray pixels
        /// </summary>
        /// <param name="src"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void FillUnManagedBorder(byte[] src, int width, int height, byte value, int borderThickness)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(src, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            SIMD.SimdFill(ptrSrc, width, width, borderThickness, 1, value);

            IntPtr downBorderPtr = IntPtr.Add(ptrSrc, (height - borderThickness) * width);

            SIMD.SimdFill(downBorderPtr, width, width, borderThickness, 1, value);

            SIMD.SimdFill(ptrSrc, width, borderThickness, height, 1, value);

            IntPtr rightBorderPtr = IntPtr.Add(ptrSrc, width - borderThickness);

            SIMD.SimdFill(rightBorderPtr, width, borderThickness, height, 1, value);

            pinnedSrc.Free();
        }

        /// <summary>
        /// 'src' must be non-strided and 8-bit gray pixels
        /// </summary>
        /// <param name="src"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void FillUnManagedBorder(short[] src, int width, int height, byte value, int borderThickness)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(src, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            SIMD.SimdFill(ptrSrc, width * 2, width, borderThickness, 2, value);

            IntPtr downBorderPtr = IntPtr.Add(ptrSrc, (height - borderThickness) * width * 2);

            SIMD.SimdFill(downBorderPtr, width * 2, width, borderThickness, 2, value);

            SIMD.SimdFill(ptrSrc, width * 2, borderThickness, height, 2, value);

            IntPtr rightBorderPtr = IntPtr.Add(ptrSrc, width * 2 - borderThickness * 2);

            SIMD.SimdFill(rightBorderPtr, width * 2, borderThickness, height, 2, value);

            pinnedSrc.Free();
        }

        public static short[] SobelDx(byte[] srcBuffer, int width, int height, int stride, short[] destination)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(srcBuffer, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedResult = GCHandle.Alloc(destination, GCHandleType.Pinned);
            IntPtr ptrResult = pinnedResult.AddrOfPinnedObject();

            SIMD.SimdSobelDx(ptrSrc, stride, width, height, ptrResult, width * 2);

            pinnedSrc.Free();
            pinnedResult.Free();

            return destination;
        }

        public static short[] SobelDy(byte[] srcBuffer, int width, int height, int stride, short[] destination)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(srcBuffer, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedResult = GCHandle.Alloc(destination, GCHandleType.Pinned);
            IntPtr ptrResult = pinnedResult.AddrOfPinnedObject();

            SIMD.SimdSobelDy(ptrSrc, stride, width, height, ptrResult, width * 2);

            pinnedSrc.Free();
            pinnedResult.Free();

            return destination;
        }

        public static void GaussBlur(byte[] gray8bitPixels, int width, int height, float sigma, int dimension, byte[] gaussBlur)
        {
            float epsilon = (float)(1 / Math.Pow(10, Math.Pow((dimension + 2) / (2 * sigma), 2)));
            if (epsilon < 0.000001f) // Error from 'SimdGaussianBlurInit' otherwise
                epsilon = 0.000001f;

            GCHandle pinnedSigma = GCHandle.Alloc(sigma, GCHandleType.Pinned);
            IntPtr sigmaPtr = pinnedSigma.AddrOfPinnedObject();

            GCHandle pinnedEpsilon = GCHandle.Alloc(epsilon, GCHandleType.Pinned);
            IntPtr epsilonPtr = pinnedEpsilon.AddrOfPinnedObject();

            GCHandle pinnedGaussBlur = GCHandle.Alloc(gaussBlur, GCHandleType.Pinned);
            IntPtr _8bitGaussBlur = pinnedGaussBlur.AddrOfPinnedObject();

            GCHandle pinnedGrayPixels = GCHandle.Alloc(gray8bitPixels, GCHandleType.Pinned);
            IntPtr _8bitGrayPixels = pinnedGrayPixels.AddrOfPinnedObject();

            IntPtr gaussKernel = SIMD.SimdGaussianBlurInit(width, height, 1, sigmaPtr, epsilonPtr);
            try
            {
                SIMD.SimdGaussianBlurRun(gaussKernel, _8bitGrayPixels, width, _8bitGaussBlur, width);
            }
            finally
            {
                SIMD.SimdRelease(gaussKernel);
            }

            pinnedGrayPixels.Free();
            pinnedGaussBlur.Free();
        }

        public static void MedianFilter(byte[] srcBuffer, int width, int height, int stride, int channelCount, byte[] destination)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(srcBuffer, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedResult = GCHandle.Alloc(destination, GCHandleType.Pinned);
            IntPtr ptrResult = pinnedResult.AddrOfPinnedObject();

            SIMD.SimdMedianFilterRhomb5x5(ptrSrc, stride, width, height, channelCount, ptrResult, stride);

            pinnedSrc.Free();
            pinnedResult.Free();
        }

        /// <summary>
        /// Destroy the content of the srcBuffer
        /// </summary>
        public static void BgraToGrayAndChangeColorAndToBgra(byte[] srcBuffer, int width, int height, int srcStride, byte[] colors, byte[] destBuffer, int destStride, byte[] tempGrayBuffer)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(srcBuffer, GCHandleType.Pinned);
            IntPtr srcPtr = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedResult = GCHandle.Alloc(destBuffer, GCHandleType.Pinned);
            IntPtr resultPtr = pinnedResult.AddrOfPinnedObject();

            GCHandle pinnedGrayBuffer = GCHandle.Alloc(tempGrayBuffer, GCHandleType.Pinned);
            IntPtr grayPtr = pinnedGrayBuffer.AddrOfPinnedObject();

            GCHandle pinnedColors = GCHandle.Alloc(colors, GCHandleType.Pinned);
            IntPtr colorsPtr = pinnedColors.AddrOfPinnedObject();

            SIMD.SimdBgraToGray(srcPtr, width, height, srcStride, grayPtr, width);
            SIMD.SimdChangeColors(grayPtr, width, width, height, colorsPtr, srcPtr, srcStride);
            SIMD.SimdGrayToBgra(srcPtr, width, height, srcStride, resultPtr, destStride);

            pinnedColors.Free();
            pinnedSrc.Free();
            pinnedResult.Free();
            pinnedGrayBuffer.Free();
        }

        public static void ByteArraysDiff(byte[] array1, int array1Stride, byte[] array2, int array2Stride, int width, int height, int channelCount, byte[] result, int resultStride)
        {
            GCHandle pinnedArray1 = GCHandle.Alloc(array1, GCHandleType.Pinned);
            IntPtr array1Ptr = pinnedArray1.AddrOfPinnedObject();

            GCHandle pinnedArray2 = GCHandle.Alloc(array2, GCHandleType.Pinned);
            IntPtr array2Ptr = pinnedArray2.AddrOfPinnedObject();

            GCHandle pinnedResult = GCHandle.Alloc(result, GCHandleType.Pinned);
            IntPtr resultPtr = pinnedResult.AddrOfPinnedObject();

            SIMD.SimdOperationBinary8u(array1Ptr, array1Stride, array2Ptr, array2Stride, width, height, channelCount, resultPtr, resultStride, (nint)ESimdOperationBinary8uType.SimdOperationBinary8uSaturatedSubtraction);

            pinnedArray1.Free();
            pinnedArray2.Free();
            pinnedResult.Free();
        }

        public static void SegmentationPropagate2x2(byte[] parentBuffer, int parentWidth, int parentHeight, int parentStride, byte[] childBuffer, int childWidth, int childHeight, int childStride, byte[] differenceBuffer, int differenceStride)
        {
            GCHandle pinnedParent = GCHandle.Alloc(parentBuffer, GCHandleType.Pinned);
            IntPtr parentPtr = pinnedParent.AddrOfPinnedObject();

            GCHandle pinnedChild = GCHandle.Alloc(childBuffer, GCHandleType.Pinned);
            IntPtr childPtr = pinnedChild.AddrOfPinnedObject();

            GCHandle pinnedDifference = GCHandle.Alloc(differenceBuffer, GCHandleType.Pinned);
            IntPtr differencePtr = pinnedDifference.AddrOfPinnedObject();

            SIMD.SimdSegmentationPropagate2x2(parentPtr, parentStride, parentWidth, parentHeight, childPtr, childStride, differencePtr, differenceStride, 0, 0, 0, 0);

            pinnedDifference.Free();
            pinnedParent.Free();
            pinnedChild.Free();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src">Must bit a 8 bit thresholded picture</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>A default rectangle if there is no white points, or the bounding box</returns>
        public static Rectangle? CalculateBoundingBox(byte[] src, int width, int height, int beginning)
        {
            int minX = width - 1, minY = height - 1, maxX = 0, maxY = 0;
            int length = width * height;
            int x, y;
            for(int i = beginning * width; i < length; i++)
            {
                if (src[i] == 255)
                {
                    x = i % width;
                    y = i / width;

                    if (x < minX)
                        minX = x;
                    if (x > maxX)
                        maxX = x;

                    if (y < minY)
                        minY = y;
                    if (y > maxY)
                        maxY = y;
                }
            }
            if (minX == width - 1 && minY == height - 1 && maxX == 0 && maxY == 0 || maxX == minX || maxY == minY)
                return null;
            return new(minX, minY, maxX - minX, maxY - minY);
        }

        public static void DrawRectangleOnBgra32bits(byte[] src, int width, int height, int stride, byte r, byte g, byte b, Rectangle rectangle)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(src, GCHandleType.Pinned);
            IntPtr upperLeft = IntPtr.Add(pinnedSrc.AddrOfPinnedObject(), rectangle.Top * stride + rectangle.Left * 4);

            SIMD.SimdFillBgra(upperLeft, stride, rectangle.Width, 1, b, g, r, 255);
            SIMD.SimdFillBgra(upperLeft, stride, 1, rectangle.Height, b, g, r, 255);

            IntPtr upperRight = IntPtr.Add(upperLeft, rectangle.Width * 4);
            SIMD.SimdFillBgra(upperRight, stride, 1, rectangle.Height, b, g, r, 255);

            IntPtr bottomLeft = IntPtr.Add(upperLeft, stride * rectangle.Height);
            SIMD.SimdFillBgra(bottomLeft, stride, rectangle.Width, 1, b, g, r, 255);

            pinnedSrc.Free();
        }
    }
}
