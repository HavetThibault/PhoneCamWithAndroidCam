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
        public static extern void SimdGrayToBgr(IntPtr gray, nint width, nint height, nint grayStride, IntPtr bgr, nint bgrStride); 	

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