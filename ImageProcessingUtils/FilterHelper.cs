using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ImageProcessingUtils
{
    public static class FilterHelper
    {
        public static void CropBgra32BitsAndToGray(byte[] source, Rectangle cropArea, int stride, byte[] destination)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(source, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedDest = GCHandle.Alloc(destination, GCHandleType.Pinned);
            IntPtr ptrDest = pinnedDest.AddrOfPinnedObject();

            IntPtr upperLeftCorner = IntPtr.Add(ptrSrc, cropArea.X * 4 + cropArea.Y * stride);

            SIMD.SimdBgraToGray(upperLeftCorner, cropArea.Width, cropArea.Height, stride, ptrDest, cropArea.Width);

            pinnedSrc.Free();
            pinnedDest.Free();
        }

        public static void CropBgr24BitsAndToGray(byte[] source, Rectangle cropArea, int stride, byte[] destination)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(source, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            GCHandle pinnedDest = GCHandle.Alloc(destination, GCHandleType.Pinned);
            IntPtr ptrDest = pinnedDest.AddrOfPinnedObject();

            IntPtr upperLeftCorner = IntPtr.Add(ptrSrc, cropArea.X * 3 + cropArea.Y * stride);

            SIMD.SimdBgrToGray(upperLeftCorner, cropArea.Width, cropArea.Height, stride, ptrDest, cropArea.Width);

            pinnedSrc.Free();
            pinnedDest.Free();
        }

        public static void Threshold(byte[] source, byte thresholdValue, byte[] dest)
        {
            for(int i = 0; i < source.Length; i++)
            {
                if (source[i] > thresholdValue)
                    dest[i] = 255;
                else
                    dest[i] = 0;
            }
        }

        public static void Dilation(byte[] source, int width, int height, byte[] dest, int size)
        {
            int xkernelOffset = (size - 1) / 2;
            int ykernelOffset = (size - 1) / 2;
            int yOffset;

            int heightLessYOffset = height - ykernelOffset;
            int widthLessXOffset = width - xkernelOffset;
            for (int i = ykernelOffset; i < heightLessYOffset; i++)
            {
                for (int j = xkernelOffset; j < widthLessXOffset; j++)
                {
                    byte value = 255;

                    for (int y = i - ykernelOffset; y <= i + ykernelOffset; y++)
                    {
                        yOffset = y * width;

                        for (int x = j - xkernelOffset; x <= j + xkernelOffset; x++)
                        {
                            value = Math.Min(value, source[yOffset + x]);
                        }
                    }
                    dest[j + i * width] = value;
                }
            }
            FillUnManagedBorder(dest, width, height, 0, (size - 1) / 2);
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

        public static void DoubleThreshold(byte[] source, byte min, byte max, byte[] destination, byte weakValue, byte notPixelValue = 0, byte strongPixelValue = 255)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] >= max)
                    destination[i] = strongPixelValue;
                else if (source[i] >= min)
                    destination[i] = weakValue;
                else
                    destination[i] = notPixelValue;
            }
        }

        public static void Hysteresis(byte[] pixelsBuffer, int width, int height, byte[] destination, byte weakValue, byte notPixelvalue = 0, byte strongValue = 255)
        {
            int offset;
            bool strongPixelFound;
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    offset = x + y * width;

                    if (pixelsBuffer[offset] == weakValue)
                    {
                        strongPixelFound = false;
                        for (int i = offset - 1; i <= offset + 1; i++)
                        {
                            for (int j = offset - 1; j < offset + 1; j++)
                            {
                                if (pixelsBuffer[j] == strongValue)
                                    strongPixelFound = true;
                            }
                        }

                        if (strongPixelFound)
                            destination[offset] = strongValue;
                        else
                            destination[offset] = notPixelvalue;
                    }
                    else
                        destination[offset] = pixelsBuffer[offset];
                }
            }
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

        public static void Fill(int[] src, int width, int height, int value)
        {
            GCHandle pinnedSrc = GCHandle.Alloc(src, GCHandleType.Pinned);
            IntPtr ptrSrc = pinnedSrc.AddrOfPinnedObject();

            SIMD.SimdFill(ptrSrc, width, width, height, 4, value);

            pinnedSrc.Free();
        }

        /// <summary>
        /// Begin to look for a pixel that is greater than 128 from the center, and turn around the center.
        /// When a point matching the condition is found, a few more iteration are made to check if there is'nt a point closer to the center
        /// (as we are going trough the picture making 'square').
        /// This function is made for square picture so 'width' and 'height' should be close 
        /// </summary>
        public static Point? BestPointEscargot(byte[] pixelsBuffer, int width, int height, bool findFirstWhite)
        {
            int calcOffset;

            int iterationsInDirections = 0;
            int tempSideNbrDone = 0;
            int maxIterationsInDirection = 1;
            Point direction = new(0, 1);
            Rectangle area = new(0, 0, width, height);
            Point center = RectangleCenter(area);
            bool foundPoint = false;
            Point currentPosition;
            Point? bestPoint = null;
            int sideNbr = 0;
            byte valueMin, valueMax;

            if (findFirstWhite)
            {
                valueMin = 128;
                valueMax = 255;
            }
            else
            {
                valueMin = 0;
                valueMax = 128;
            }

            for (currentPosition = new(center.X, center.Y); area.Contains(currentPosition) && !foundPoint; iterationsInDirections++)
            {
                calcOffset = currentPosition.X + currentPosition.Y * width;

                if (pixelsBuffer[calcOffset] >= valueMin && pixelsBuffer[calcOffset] <= valueMax)
                {
                    bestPoint = new(currentPosition.X, currentPosition.Y);
                    foundPoint = true;
                }

                if (iterationsInDirections >= maxIterationsInDirection)
                {
                    iterationsInDirections = 0;
                    direction = NextDirectionClockWise(direction);
                    tempSideNbrDone++;
                    sideNbr++;

                    if (tempSideNbrDone == 2)
                    {
                        tempSideNbrDone = 0;
                        maxIterationsInDirection++;
                    }
                }

                currentPosition.X += direction.X;
                currentPosition.Y += direction.Y;
            }

            if (bestPoint == null)
                return null;

            Point areaCenter = RectangleCenter(area);
            double bestDistance = GetDistance(areaCenter, (Point)bestPoint);

            int sideNbrToDo = (int)Math.Round(sideNbr * 0.3) + sideNbr;

            for (; area.Contains(currentPosition) && sideNbr <= sideNbrToDo; iterationsInDirections++)
            {
                calcOffset = currentPosition.X + currentPosition.Y * width;

                if (pixelsBuffer[calcOffset] >= valueMin && pixelsBuffer[calcOffset] <= valueMax && GetDistance(areaCenter, currentPosition) < bestDistance)
                {
                    bestDistance = GetDistance(areaCenter, currentPosition);
                    bestPoint = new(currentPosition.X, currentPosition.Y);
                }

                if (iterationsInDirections >= maxIterationsInDirection)
                {
                    iterationsInDirections = 0;
                    direction = NextDirectionClockWise(direction);
                    tempSideNbrDone++;
                    sideNbr++;

                    if (tempSideNbrDone == 2)
                    {
                        tempSideNbrDone = 0;
                        maxIterationsInDirection++;
                    }
                }

                currentPosition.X += direction.X;
                currentPosition.Y += direction.Y;
            }

            return bestPoint;
        }

        public static Point NextDirectionClockWise(Point direction)
        {
            return new Point(direction.Y, -direction.X);
        }

        public static Point RectangleCenter(Rectangle rectangle)
        {
            return new Point(rectangle.Location.X + (int)Math.Round(rectangle.Width / 2.0), rectangle.Location.Y + (int)Math.Round(rectangle.Height / 2.0));
        }

        public static double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        /// <summary>
        /// Used to make edge found by the sobel gradient <paramref name="gradientMagnitudeBuffer"/> thinner. 
        /// </summary>
        public static void FindLocalMaxima(double[] gradientMagnitudeBuffer, double[] gradientDirectionBuffer, int width, int height, byte[] destination, out byte max)
        {
            max = 0;
            int position, neighbor1, neighbor2;
            double angle;
            for (int x = 2; x < width - 2; x++)
            {
                for (int y = 2; y < height - 2; y++)
                {
                    position = x + y * width;
                    angle = gradientDirectionBuffer[position];

                    if (angle is <= -157.5 or >= 157.5)
                    {
                        neighbor1 = position - width;
                        neighbor2 = position + width;
                    }
                    else if (angle <= -112.5)
                    {
                        neighbor1 = position - 1 - width;
                        neighbor2 = position + 1 + width;
                    }
                    else if (angle <= -67.5)
                    {
                        neighbor1 = position - 1;
                        neighbor2 = position + 1;
                    }
                    else if (angle <= -22.5)
                    {
                        neighbor1 = position - 1 + width;
                        neighbor2 = position + 1 - width;
                    }
                    else if (angle <= 22.5)
                    {
                        neighbor1 = position - width;
                        neighbor2 = position + width;
                    }
                    else if (angle <= 67.5)
                    {
                        neighbor1 = position - 1 - width;
                        neighbor2 = position + 1 + width;
                    }
                    else if (angle <= 112.5)
                    {
                        neighbor1 = position - 1;
                        neighbor2 = position + 1;
                    }
                    else
                    {
                        neighbor1 = position - 1 + width;
                        neighbor2 = position + 1 - width;
                    }

                    if (gradientMagnitudeBuffer[position] >= gradientMagnitudeBuffer[neighbor1] && gradientMagnitudeBuffer[position] >= gradientMagnitudeBuffer[neighbor2]) // if the pixel at 'position' is a maxima
                    {
                        destination[position] = (byte)(gradientMagnitudeBuffer[position] > 255 ? 255 : gradientMagnitudeBuffer[position]);

                        if (destination[position] > max)
                            max = destination[position];
                    }
                    else
                        destination[position] = 0;
                }
            }

            SIMDHelper.FillUnManagedBorder(destination, width, height, 0, 2);
        }

        public static void AbsSubstract(byte[] a, byte[] b, byte[] dest)
        {
            for(int i = 0; i < a.Length; i++)
            {
                dest[i] = (byte)Math.Abs(a[i] - b[i]);
            }
        }

        /// <summary>
        /// For non-strided dest32Bits
        /// </summary>
        /// <param name="motion8bits"></param>
        /// <param name="dest32bits"></param>
        /// <param name="stride"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void SetRedComponentIfMotion(byte[] motion8bits, byte[] dest32bits, int width, int height)
        {
            int length = width * height;
            for(int i = 0; i < length; i++)
            {
                if (motion8bits[i] == 255)
                    dest32bits[((i % width) * 4) + i / width * width * 4 + 2] = 255;
            }
        }
    }
}
