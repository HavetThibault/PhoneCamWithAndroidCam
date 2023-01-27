using ImageProcessingUtils;
using NUnit.Framework.Internal;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PhoneCamWithAndroidCamTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void TestPropagate2x2()
        {
            SIMD.LoadAssembly();
            Bitmap img = new(@"D:\Thibault.Person\FondEcran\GutsWolf.jpg");
            var canvas = new Rectangle(0, 0, img.Width, img.Height);
            BitmapData srcData = img.LockBits(canvas, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            byte[] dest = new byte[stride * img.Height];
            GCHandle pinnedDestBuffer = GCHandle.Alloc(dest, GCHandleType.Pinned);
            IntPtr destBufferPtr = pinnedDestBuffer.AddrOfPinnedObject();

            SIMD.SimdCopy(srcData.Scan0, stride, srcData.Width, srcData.Height, 4, destBufferPtr, srcData.Width * 4);

            img.UnlockBits(srcData);
            pinnedDestBuffer.Free();

            byte[] grayBuffer = new byte[img.Height * img.Width];
            FilterHelper.CropBgra32BitsAndToGray(dest, canvas, stride, grayBuffer);

            BitmapHelper.FromGrayBufferToBitmap(img, grayBuffer, img.Width, img.Height);
            img.Save(@"C:\Users\Thibault\Downloads\Save1.png");
            SIMD.UnloadAssembly();
        }
    }
}