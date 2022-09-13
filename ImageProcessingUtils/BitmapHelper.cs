using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageProcessingUtils
{
    public static class BitmapHelper
    {
        public static void ToByteArray(Bitmap srcImage, out int stride, byte[] dest)
        {
            var canvas = new Rectangle(0, 0, srcImage.Width, srcImage.Height);
            BitmapData srcData = srcImage.LockBits(canvas, ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);

            stride = srcData.Stride;

            GCHandle pinnedDestBuffer = GCHandle.Alloc(dest, GCHandleType.Pinned);
            IntPtr destBufferPtr = pinnedDestBuffer.AddrOfPinnedObject();

            SIMD.SimdCopy(srcData.Scan0, stride, srcData.Width, srcData.Height, 4, destBufferPtr, srcData.Width * 4);

            srcImage.UnlockBits(srcData);
            pinnedDestBuffer.Free();
        }

        public static void FromBgraBufferToBitmap(Bitmap destBitmap, byte[] buffer, int width, int height)
        {
            BitmapData srcData = destBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            GCHandle pinnedSrcBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr srcBufferPtr = pinnedSrcBuffer.AddrOfPinnedObject();

            SIMD.SimdCopy(srcBufferPtr, width * 4, width, height, 4, srcData.Scan0, srcData.Stride);

            destBitmap.UnlockBits(srcData);
            pinnedSrcBuffer.Free();
        }
    }
}
