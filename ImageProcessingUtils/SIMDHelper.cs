using System.Runtime.InteropServices;

namespace ImageProcessingUtils
{
    public static class SIMDHelper
    {

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
        public static void FillUnManagedBorder(byte[] src, int width, int height, int value, int borderThickness)
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
        public static void FillUnManagedBorder(short[] src, int width, int height, int value, int borderThickness)
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
    }
}
