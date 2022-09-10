using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageProcessingUtils
{
    public static class SIMDHelper
    {

        /// <summary>
        /// 'src' must be non-strided and 8-bit gray pixels, fill 1 pixels thikness border with 'value'
        /// </summary>
        /// <param name="src"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void FillUnManagedBorder(short[] src, int width, int height, int value)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(src, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            SIMD.SimdFill(ptrSrc, width * 2, width, 1, 2, value);

            IntPtr downBorderPtr = IntPtr.Add(ptrSrc, (height - 1) * width * 2);

            SIMD.SimdFill(downBorderPtr, width * 2, width, 1, 2, value);

            SIMD.SimdFill(ptrSrc, width * 2, 1, height, 2, value);

            IntPtr rightBorderPtr = IntPtr.Add(ptrSrc, width * 2 - 2);

            SIMD.SimdFill(rightBorderPtr, width * 2, 1, height, 2, value);

            pinnedSrc.Free();
        }

        internal static short[] SimdContourMetrics(byte[] srcBuffer, int width, int height)
        {
            short[] metricsBuffer = new short[width * height];

            GCHandle pinnedSrc = GCHandle.Alloc(srcBuffer, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedDst = GCHandle.Alloc(metricsBuffer, GCHandleType.Pinned);
            IntPtr ptrDst = pinnedDst.AddrOfPinnedObject();

            SIMD.SimdContourMetrics(ptrSrc, width, width, height, ptrDst, width * 2);

            pinnedSrc.Free();
            pinnedDst.Free();

            return metricsBuffer;
        }

        internal static byte[] SimdContourAnchor(short[] contourMetrics, int width, int height)
        {
            byte[] anchorMetricsBuffer = new byte[width * height];

            GCHandle pinnedSrc = GCHandle.Alloc(contourMetrics, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedDst = GCHandle.Alloc(anchorMetricsBuffer, GCHandleType.Pinned);
            IntPtr ptrDst = pinnedDst.AddrOfPinnedObject();

            SIMD.SimdContourAnchors(ptrSrc, width * 2, width, height, 1, 6, ptrDst, width);

            pinnedSrc.Free();
            pinnedDst.Free();

            return anchorMetricsBuffer;
        }

        /// <summary>
        /// 'src' must be non-strided and 8-bit gray pixels, fill 1 pixels thikness border with 'value'
        /// </summary>
        /// <param name="src"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void FillUnManagedBorder(byte[] src, int width, int height, int value)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(src, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            SIMD.SimdFill(ptrSrc, width, width, 1, 1, value);

            IntPtr downBorderPtr = IntPtr.Add(ptrSrc, (height - 1) * width);

            SIMD.SimdFill(downBorderPtr, width, width, 1, 1, value);

            SIMD.SimdFill(ptrSrc, width, 1, height, 1, value);

            IntPtr rightBorderPtr = IntPtr.Add(ptrSrc, width - 1);

            SIMD.SimdFill(rightBorderPtr, width, 1, height, 1, value);

            pinnedSrc.Free();
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

            IntPtr rightBorderPtr = IntPtr.Add(ptrSrc, width - borderThickness);

            SIMD.SimdFill(rightBorderPtr, width * 2, borderThickness, height, 2, value);

            pinnedSrc.Free();
        }

        public static byte[] CropBgraAndToGray(byte[] source, Rectangle cropArea, int stride, byte[] destination)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(source, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            int leftX = cropArea.X * 4;

            GCHandle pinnedDest = GCHandle.Alloc(destination, GCHandleType.Pinned);
            IntPtr ptrDest = pinnedDest.AddrOfPinnedObject();

            IntPtr currentPtrSrc;
            for (int i = cropArea.Y; i < cropArea.Y + cropArea.Height; i++)
            {
                currentPtrSrc = IntPtr.Add(ptrSrc, i * stride + leftX);
                SIMD.SimdBgraToGray(currentPtrSrc, cropArea.Width, 1, stride, ptrDest, cropArea.Width);
                ptrDest = IntPtr.Add(ptrDest, cropArea.Width);
            }

            pinnedSrc.Free();
            pinnedDest.Free();

            return destination;
        }

        public static byte[] CropBgraAndToGrayv2(byte[] source, Rectangle cropArea, int stride, byte[] destination)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(source, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedDest = GCHandle.Alloc(destination, GCHandleType.Pinned);
            IntPtr ptrDest = pinnedDest.AddrOfPinnedObject();

            IntPtr upperLeftCorner = IntPtr.Add(ptrSrc, cropArea.X * 4 + cropArea.Y * stride);

            SIMD.SimdBgraToGray(upperLeftCorner, cropArea.Width, cropArea.Height, stride, ptrDest, cropArea.Width);

            pinnedSrc.Free();
            pinnedDest.Free();

            return destination;
        }

        public static byte[] CropBgrAndToGray(byte[] source, Rectangle cropArea, int stride)
        {
            byte[] result = new byte[cropArea.Width * cropArea.Height];

            int leftX = cropArea.X * 3;

            GCHandle pinnedSrc = GCHandle.Alloc(source, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedDest = GCHandle.Alloc(result, GCHandleType.Pinned);
            IntPtr ptrDest = pinnedDest.AddrOfPinnedObject();

            IntPtr currentPtrSrc;
            for (int i = cropArea.Y; i < cropArea.Y + cropArea.Height; i++)
            {
                currentPtrSrc = IntPtr.Add(ptrSrc, i * stride + leftX);
                SIMD.SimdBgrToGray(currentPtrSrc, cropArea.Width, 1, stride, ptrDest, cropArea.Width);
                ptrDest = IntPtr.Add(ptrDest, cropArea.Width);
            }

            pinnedSrc.Free();
            pinnedDest.Free();

            return result;
        }

        public static short[] SobelDx(byte[] srcBuffer, int width, int height, int stride)
        {
            short[] result = new short[width * height];

            GCHandle pinnedSrc = GCHandle.Alloc(srcBuffer, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedResult = GCHandle.Alloc(result, GCHandleType.Pinned);
            IntPtr ptrResult = pinnedResult.AddrOfPinnedObject();

            SIMD.SimdSobelDx(ptrSrc, stride, width, height, ptrResult, width * 2);

            pinnedSrc.Free();
            pinnedResult.Free();

            return result;
        }

        public static short[] SobelDy(byte[] srcBuffer, int width, int height, int stride)
        {
            short[] result = new short[width * height];

            GCHandle pinnedSrc = GCHandle.Alloc(srcBuffer, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedResult = GCHandle.Alloc(result, GCHandleType.Pinned);
            IntPtr ptrResult = pinnedResult.AddrOfPinnedObject();

            SIMD.SimdSobelDy(ptrSrc, stride, width, height, ptrResult, width * 2);

            pinnedSrc.Free();
            pinnedResult.Free();

            return result;
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

        public static byte[] SimdGrayToBgra(byte[] _8bitgrayPixels, int width, int height, int grayStride)
        {
            GCHandle pinnedGray = GCHandle.Alloc(_8bitgrayPixels, GCHandleType.Pinned);
            IntPtr ptrGray = pinnedGray.AddrOfPinnedObject();

            byte[] bgra = new byte[width * 4 * height];
            GCHandle pinnedBgra = GCHandle.Alloc(bgra, GCHandleType.Pinned);
            IntPtr ptrbgra = pinnedBgra.AddrOfPinnedObject();

            SIMD.SimdGrayToBgra(ptrGray, width, height, grayStride, ptrbgra, width * 4, 255);

            pinnedGray.Free();
            pinnedBgra.Free();

            return bgra;
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

        public static void SimdBgraToGray(byte[] _32bitbgrapixels, int width, int height, int bgraStride, byte[] _8bitgreypixels)
        {
            GCHandle grayPinnedArray = GCHandle.Alloc(_8bitgreypixels, GCHandleType.Pinned);
            IntPtr _8bitGrayPixels = grayPinnedArray.AddrOfPinnedObject();

            GCHandle srcPinnedArray = GCHandle.Alloc(_32bitbgrapixels, GCHandleType.Pinned);
            IntPtr srcPtr = srcPinnedArray.AddrOfPinnedObject();

            SIMD.SimdBgraToGray(srcPtr, width, height, bgraStride, _8bitGrayPixels, width);

            srcPinnedArray.Free();
            grayPinnedArray.Free();
        }

        public static byte[] CropBgraAndToGray(byte[] source, Rectangle cropArea, int stride)
        {
            byte[] result = new byte[cropArea.Width * cropArea.Height];

            GCHandle pinnedSrc = GCHandle.Alloc(source, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            int leftX = cropArea.X * 4;

            GCHandle pinnedDest = GCHandle.Alloc(result, GCHandleType.Pinned);
            IntPtr ptrDest = pinnedDest.AddrOfPinnedObject();

            IntPtr currentPtrSrc;
            for (int i = cropArea.Y; i < cropArea.Y + cropArea.Height; i++)
            {
                currentPtrSrc = IntPtr.Add(ptrSrc, i * stride + leftX);
                SIMD.SimdBgraToGray(currentPtrSrc, cropArea.Width, 1, stride, ptrDest, cropArea.Width);
                ptrDest = IntPtr.Add(ptrDest, cropArea.Width);
            }

            pinnedSrc.Free();
            pinnedDest.Free();

            return result;
        }

        public static byte[] SimdGradientMagnitudeToArgb(double[] magnitude)
        {
            byte[] argbBuffer = new byte[magnitude.Length * 4];
            byte value;
            for (int i = 0; i < magnitude.Length; i++)
            {
                value = (byte)(magnitude[i] > 255 ? 255 : magnitude[i]);

                argbBuffer[i * 4] = value;
                argbBuffer[i * 4 + 1] = value;
                argbBuffer[i * 4 + 2] = value;
                argbBuffer[i * 4 + 3] = 255;
            }
            return argbBuffer;
        }

        public static Bitmap ToGrayscaleBitmap(Bitmap srcImage)
        {
            int height = srcImage.Height;
            int width = srcImage.Width;

            Rectangle canvas = new(0, 0, width, height);
            BitmapData originalPicData = srcImage.LockBits(canvas, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);

            byte[] pixelsBuffer = new byte[width * height];
            GCHandle pinnedArray = GCHandle.Alloc(pixelsBuffer, GCHandleType.Pinned);
            IntPtr _8bitGrayPixels = pinnedArray.AddrOfPinnedObject();

            SIMD.SimdBgraToGray(originalPicData.Scan0, width, height, originalPicData.Stride, _8bitGrayPixels, width);

            srcImage.UnlockBits(originalPicData);

            Bitmap grayPic = new(width, height, PixelFormat.Format32bppRgb);
            BitmapData grayData = grayPic.LockBits(canvas, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            byte[] gray32bitPixelsBuffer = new byte[grayData.Stride * height];
            int offsetResult, offsetgray8;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    offsetResult = i * grayData.Stride + j * 4;
                    offsetgray8 = i * width + j;
                    gray32bitPixelsBuffer[offsetResult] = pixelsBuffer[offsetgray8];
                    gray32bitPixelsBuffer[offsetResult + 1] = pixelsBuffer[offsetgray8];
                    gray32bitPixelsBuffer[offsetResult + 2] = pixelsBuffer[offsetgray8];
                    gray32bitPixelsBuffer[offsetResult + 3] = pixelsBuffer[offsetgray8];
                }
            }

            pinnedArray.Free();
            grayPic.UnlockBits(grayData);

            return grayPic;
        }

        public static unsafe byte[] GaussBlur(byte[] gray8bitPixels, int width, int height, float sigma, float epsilon = .001f)
        {
            void* gaussKernel = SIMD.SimdGaussianBlurInit(width, height, 1, &sigma, &epsilon);
            byte[] gaussBlur = new byte[gray8bitPixels.Length];
            GCHandle pinnedGrayPixels = GCHandle.Alloc(gray8bitPixels, GCHandleType.Pinned);
            IntPtr _8bitGrayPixels = pinnedGrayPixels.AddrOfPinnedObject();

            GCHandle pinnedGaussBlur = GCHandle.Alloc(gaussBlur, GCHandleType.Pinned);
            IntPtr _8bitGaussBlur = pinnedGaussBlur.AddrOfPinnedObject();

            SIMD.SimdGaussianBlurRun(gaussKernel, _8bitGrayPixels, width, _8bitGaussBlur, width);

            SIMD.SimdRelease(gaussKernel);

            pinnedGrayPixels.Free();
            pinnedGaussBlur.Free();

            return gaussBlur;
        }

        public static unsafe byte[] GaussBlur(byte[] gray8bitPixels, int width, int height, float sigma, int dimension)
        {
            float epsilon = (float)(1 / Math.Pow(10, Math.Pow((dimension + 2) / (2 * sigma), 2)));
            if (epsilon < 0.000001f) // Error otherwise
                epsilon = 0.000001f;
            void* gaussKernel = SIMD.SimdGaussianBlurInit(width, height, 1, &sigma, &epsilon);
            byte[] gaussBlur = new byte[gray8bitPixels.Length];
            GCHandle pinnedGrayPixels = GCHandle.Alloc(gray8bitPixels, GCHandleType.Pinned);
            IntPtr _8bitGrayPixels = pinnedGrayPixels.AddrOfPinnedObject();

            GCHandle pinnedGaussBlur = GCHandle.Alloc(gaussBlur, GCHandleType.Pinned);
            IntPtr _8bitGaussBlur = pinnedGaussBlur.AddrOfPinnedObject();

            SIMD.SimdGaussianBlurRun(gaussKernel, _8bitGrayPixels, width, _8bitGaussBlur, width);

            SIMD.SimdRelease(gaussKernel);

            pinnedGrayPixels.Free();
            pinnedGaussBlur.Free();

            return gaussBlur;
        }

        public static unsafe void GaussBlur(byte[] gray8bitPixels, int width, int height, float sigma, float epsilon, byte[] gaussBlur)
        {
            void* gaussKernel = SIMD.SimdGaussianBlurInit(width, height, 1, &sigma, &epsilon);
            GCHandle pinnedGrayPixels = GCHandle.Alloc(gray8bitPixels, GCHandleType.Pinned);
            IntPtr _8bitGrayPixels = pinnedGrayPixels.AddrOfPinnedObject();

            GCHandle pinnedGaussBlur = GCHandle.Alloc(gaussBlur, GCHandleType.Pinned);
            IntPtr _8bitGaussBlur = pinnedGaussBlur.AddrOfPinnedObject();

            SIMD.SimdGaussianBlurRun(gaussKernel, _8bitGrayPixels, width, _8bitGaussBlur, width);

            SIMD.SimdRelease(gaussKernel);

            pinnedGrayPixels.Free();
            pinnedGaussBlur.Free();
        }

        public static unsafe void GaussBlur(byte[] gray8bitPixels, int width, int height, float sigma, int dimension, byte[] gaussBlur)
        {
            float epsilon = (float)(1 / Math.Pow(10, Math.Pow((dimension + 2) / (2 * sigma), 2)));
            if (epsilon < 0.000001f) // Error otherwise
                epsilon = 0.000001f;
            void* gaussKernel = SIMD.SimdGaussianBlurInit(width, height, 1, &sigma, &epsilon);
            GCHandle pinnedGrayPixels = GCHandle.Alloc(gray8bitPixels, GCHandleType.Pinned);
            IntPtr _8bitGrayPixels = pinnedGrayPixels.AddrOfPinnedObject();

            GCHandle pinnedGaussBlur = GCHandle.Alloc(gaussBlur, GCHandleType.Pinned);
            IntPtr _8bitGaussBlur = pinnedGaussBlur.AddrOfPinnedObject();

            SIMD.SimdGaussianBlurRun(gaussKernel, _8bitGrayPixels, width, _8bitGaussBlur, width);

            SIMD.SimdRelease(gaussKernel);

            pinnedGrayPixels.Free();
            pinnedGaussBlur.Free();
        }

        public static unsafe Bitmap GaussBlurToBitmap(byte[] gray8bitPixels, int width, int height, float sigma, float epsilon = .001f)
        {
            void* gaussKernel = SIMD.SimdGaussianBlurInit(width, height, 1, &sigma, &epsilon);
            byte[] gaussBlur = new byte[gray8bitPixels.Length];
            GCHandle pinnedGrayPixels = GCHandle.Alloc(gray8bitPixels, GCHandleType.Pinned);
            IntPtr _8bitGrayPixels = pinnedGrayPixels.AddrOfPinnedObject();

            GCHandle pinnedGaussBlur = GCHandle.Alloc(gaussBlur, GCHandleType.Pinned);
            IntPtr _8bitGaussBlur = pinnedGaussBlur.AddrOfPinnedObject();

            SIMD.SimdGaussianBlurRun(gaussKernel, _8bitGrayPixels, width, _8bitGaussBlur, width);

            SIMD.SimdRelease(gaussKernel);

            pinnedGrayPixels.Free();
            pinnedGaussBlur.Free();

            byte[] _32bitGaussBlur = ByteHelper.To32BitsFrom8bitArray(gaussBlur);

            return BitmapHelper.From32BitsBgraToBitmap(_32bitGaussBlur, width, height);
        }

        public static byte[] Resize(byte[] srcBuffer, int width, int height, int stride, int dstWidth, int dstHeight, int dstStride, int channelCount)
        {
            byte[] dstBuffer = new byte[dstStride * dstHeight];

            GCHandle pinnedSrc = GCHandle.Alloc(srcBuffer, GCHandleType.Pinned);
            IntPtr srcPtr = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedDst = GCHandle.Alloc(dstBuffer, GCHandleType.Pinned);
            IntPtr dstPtr = pinnedDst.AddrOfPinnedObject();

            SIMD.SimdResizeBilinear(srcPtr, width, height, stride, dstPtr, dstWidth, dstHeight, dstStride, channelCount);

            pinnedSrc.Free();
            pinnedDst.Free();

            return dstBuffer;
        }
    }
}
