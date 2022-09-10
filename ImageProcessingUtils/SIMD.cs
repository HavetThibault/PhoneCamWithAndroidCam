using System;
using System.Runtime.InteropServices;

namespace ImageProcessingUtils
{
    public static class SIMD
    {
        private const string SIMD_LIBRARY_FILENAME = "D:\\Programmation\\MotionDetection\\PhoneCamWithAndroidCam\\ImageProcessingUtils\\Libraries\\x64\\Simd.dll";

        private static IntPtr m_LibraryPointer = IntPtr.Zero;

        public static bool IsLoaded => m_LibraryPointer != IntPtr.Zero;


        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string filePath);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);


        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdCopy(IntPtr sourcePointer, nint sourceStride, nint width, nint height, nint pixelSize, IntPtr destinationPointer, nint destinationStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdBgrToBgra(IntPtr sourcePointer, nint width, nint height, nint sourceStride, IntPtr destinationPointer, nint destinationStride, byte alpha);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdRgbToBgra(IntPtr sourcePointer, nint width, nint height, nint sourceStride, IntPtr destinationPointer, nint destinationStride, byte alpha);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdBgrToGray(IntPtr sourcePointer, nint width, nint height, nint sourceStride, IntPtr destinationPointer, nint destinationStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdBgraToBgr(IntPtr sourcePointer, nint width, nint height, nint sourceStride, IntPtr destinationPointer, nint destinationStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdBgraToRgb(IntPtr sourcePointer, nint width, nint height, nint sourceStride, IntPtr destinationPointer, nint destinationStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdGrayToBgra(IntPtr gray, nint width, nint height, nint grayStride, IntPtr bgr, nint bgrStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdInterleaveBgr(IntPtr bluePointer, nint blueStride, IntPtr greenPointer, nint greenStride, IntPtr redPointer, nint redStride, nint imageWidth, nint imageHeight,
            IntPtr imagePointer, nint imageStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdInterleaveBgra(IntPtr bluePointer, nint blueStride, IntPtr greenPointer, nint greenStride, IntPtr redPointer, nint redStride, IntPtr alphaPointer, nint alphaStride,
            nint imageWidth, nint imageHeight, IntPtr imagePointer, nint imageStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdYuv420pToBgra(IntPtr yPointer, nint yStride, IntPtr uPointer, nint uStride, IntPtr vPointer, nint vStride, nint width, nint height, IntPtr destPointer, nint destStride, byte alpha);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdDeinterleaveUv(IntPtr uvPointer, nint uvstride, nint width, nint height, IntPtr uDestPointer, nint uStride, IntPtr vDestPointer, nint vStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdTransformImage(IntPtr sourcePointer, nint sourceStride, nint width, nint height, nint pixelSize, ESimdTransformType simdTransformType, IntPtr destinationPointer,
            nint destinationStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SimdGaussianBlurInit(int width, int height, int channels, ref float sigma, ref float epsilon);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdGaussianBlurRun(IntPtr filter, IntPtr src, int srcStride, IntPtr dst, int dstStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdRelease(IntPtr context);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdSobelDxAbsSum(IntPtr sourcePointer, int sourceStride, int width, int height, out long sum);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdSobelDyAbsSum(IntPtr sourcePointer, int sourceStride, int width, int height, out long sum);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdBgraToGray(IntPtr bgra, int width, int height, int bgraStride, IntPtr gray, int grayStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdRgbToGray(IntPtr rgb, int width, int height, int rgbStride, IntPtr gray, int grayStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdBgrToGray(IntPtr bgr, int width, int height, int bgrStride, IntPtr gray, int grayStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdGrayToBgra(IntPtr gray, int width, int height, int grayStride, IntPtr bgra, int bgraStride, int alpha);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SimdGaussianBlurInit(int width, int height, int channels, IntPtr sigma, IntPtr epsilon);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdSobelDx(IntPtr src, int srcStride, int width, int height, IntPtr dst, int dstStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdSobelDy(IntPtr src, int srcStride, int width, int height, IntPtr dst, int dstStride);

        [DllImport(SIMD_LIBRARY_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SimdFill(IntPtr dst, int stride, int width, int height, int pixelSize, int value);


        public static void Copy(byte[] sourceBuffer, byte[] destinationBuffer)
        {
            GCHandle pinnedSrcBuffer = GCHandle.Alloc(sourceBuffer, GCHandleType.Pinned);
            IntPtr srcBufferPtr = pinnedSrcBuffer.AddrOfPinnedObject();

            GCHandle pinnedDestBuffer = GCHandle.Alloc(destinationBuffer, GCHandleType.Pinned);
            IntPtr destBuffer = pinnedDestBuffer.AddrOfPinnedObject();

            SIMD.SimdCopy(srcBufferPtr, sourceBuffer.Length, sourceBuffer.Length, 1, 1, destBuffer, sourceBuffer.Length);

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

            SimdBgraToGray(bgra, width, height, bgraStride, _8bitGrayPixels, width);
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

            SimdSobelDx(ptrSrc, stride, width, height, ptrResult, width * 2);

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

            SimdSobelDy(ptrSrc, stride, width, height, ptrResult, width * 2);

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

            IntPtr gaussKernel = SimdGaussianBlurInit(width, height, 1, sigmaPtr, epsilonPtr);
            try
            {
                SimdGaussianBlurRun(gaussKernel, _8bitGrayPixels, width, _8bitGaussBlur, width);
            }
            finally
            {
                SimdRelease(gaussKernel);
            }

            pinnedGrayPixels.Free();
            pinnedGaussBlur.Free();
        }
        public static void LoadAssembly()
        {
            if (!IsLoaded) m_LibraryPointer = LoadLibrary(SIMD_LIBRARY_FILENAME);
        }

        public static void UnloadAssembly()
        {
            if (IsLoaded)
            {
                FreeLibrary(m_LibraryPointer);
                m_LibraryPointer = IntPtr.Zero;
            }
        }
    }
}