using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageprocessor.Filters
{
    internal class GaussianFilter
    {
        public Bitmap ApplyGaussianFilterFast(Bitmap bitmapOriginal) //244
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData srcData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            Bitmap resultBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData dstData = resultBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;

            unsafe
            {
                byte* srcBase = (byte*)srcData.Scan0;
                byte* dstBase = (byte*)dstData.Scan0;

                Parallel.For(1, height - 1, y =>
                {
                    byte* dstRow = dstBase + y * stride;

                    for (int x = 1; x < width - 1; x++)
                    {
                        // pointers to the 3 rows around the pixel
                        byte* rowAbove = srcBase + (y - 1) * stride + (x - 1) * 3;
                        byte* rowCurrent = srcBase + y * stride + (x - 1) * 3;
                        byte* rowBelow = srcBase + (y + 1) * stride + (x - 1) * 3;

                        // b
                        int sumB =
                            rowAbove[0] + 2 * rowAbove[3] + rowAbove[6] +
                            2 * rowCurrent[0] + 4 * rowCurrent[3] + 2 * rowCurrent[6] +
                            rowBelow[0] + 2 * rowBelow[3] + rowBelow[6];

                        // g
                        int sumG =
                            rowAbove[1] + 2 * rowAbove[4] + rowAbove[7] +
                            2 * rowCurrent[1] + 4 * rowCurrent[4] + 2 * rowCurrent[7] +
                            rowBelow[1] + 2 * rowBelow[4] + rowBelow[7];

                        // r
                        int sumR =
                            rowAbove[2] + 2 * rowAbove[5] + rowAbove[8] +
                            2 * rowCurrent[2] + 4 * rowCurrent[5] + 2 * rowCurrent[8] +
                            rowBelow[2] + 2 * rowBelow[5] + rowBelow[8];

                        byte* dstPixel = dstRow + x * 3;
                        dstPixel[0] = (byte)(sumB / 16);
                        dstPixel[1] = (byte)(sumG / 16);
                        dstPixel[2] = (byte)(sumR / 16);
                    }
                });
            }

            bitmapOriginal.UnlockBits(srcData);
            resultBitmap.UnlockBits(dstData);

            return resultBitmap;
        }

        public Bitmap ApplyGaussian9x9(Bitmap bitmapOriginal)
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData srcData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            Bitmap tempBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData tempData = tempBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;

            unsafe
            {
                byte* srcBase = (byte*)srcData.Scan0;
                byte* tmpBase = (byte*)tempData.Scan0;

                
                Parallel.For(4, height - 4, y =>
                {
                    byte* srcRow = srcBase + y * stride;
                    byte* dstRow = tmpBase + y * stride;

                    for (int x = 4; x < width - 4; x++)
                    {
                        byte* pixel = srcRow + x * 3;

                        int sumB =
                            pixel[-24] * 1 + pixel[-21] * 4 + pixel[-18] * 7 + pixel[-15] * 10 +
                            pixel[-12] * 13 + pixel[-9] * 10 + pixel[-6] * 7 + pixel[-3] * 4 + pixel[0] * 1;

                        int sumG =
                            pixel[-23] * 1 + pixel[-20] * 4 + pixel[-17] * 7 + pixel[-14] * 10 +
                            pixel[-11] * 13 + pixel[-8] * 10 + pixel[-5] * 7 + pixel[-2] * 4 + pixel[1] * 1;

                        int sumR =
                            pixel[-22] * 1 + pixel[-19] * 4 + pixel[-16] * 7 + pixel[-13] * 10 +
                            pixel[-10] * 13 + pixel[-7] * 10 + pixel[-4] * 7 + pixel[-1] * 4 + pixel[2] * 1;

                        byte* dstPixel = dstRow + x * 3;
                        dstPixel[0] = (byte)(sumB / 57); // sum of weights 1+4+7+10+13+10+7+4+1 = 57
                        dstPixel[1] = (byte)(sumG / 57);
                        dstPixel[2] = (byte)(sumR / 57);
                    }
                });

                bitmapOriginal.UnlockBits(srcData);

                Bitmap resultBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                BitmapData dstData = resultBitmap.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb);

                byte* tmpBase2 = (byte*)tempData.Scan0;
                byte* dstBase = (byte*)dstData.Scan0;

                
                Parallel.For(4, height - 4, y =>
                {
                    byte* dstRow = dstBase + y * stride;

                    for (int x = 4; x < width - 4; x++)
                    {
                        byte* center = tmpBase2 + y * stride + x * 3;

                        int sumB =
                            center[-4 * stride + 0] * 1 + center[-3 * stride + 0] * 4 + center[-2 * stride + 0] * 7 + center[-1 * stride + 0] * 10 +
                            center[0] * 13 +
                            center[1 * stride + 0] * 10 + center[2 * stride + 0] * 7 + center[3 * stride + 0] * 4 + center[4 * stride + 0] * 1;

                        int sumG =
                            center[-4 * stride + 1] * 1 + center[-3 * stride + 1] * 4 + center[-2 * stride + 1] * 7 + center[-1 * stride + 1] * 10 +
                            center[1] * 13 +
                            center[1 * stride + 1] * 10 + center[2 * stride + 1] * 7 + center[3 * stride + 1] * 4 + center[4 * stride + 1] * 1;

                        int sumR =
                            center[-4 * stride + 2] * 1 + center[-3 * stride + 2] * 4 + center[-2 * stride + 2] * 7 + center[-1 * stride + 2] * 10 +
                            center[2] * 13 +
                            center[1 * stride + 2] * 10 + center[2 * stride + 2] * 7 + center[3 * stride + 2] * 4 + center[4 * stride + 2] * 1;

                        byte* dstPixel = dstRow + x * 3;
                        dstPixel[0] = (byte)(sumB / 57);
                        dstPixel[1] = (byte)(sumG / 57);
                        dstPixel[2] = (byte)(sumR / 57);
                    }
                });

                tempBitmap.UnlockBits(tempData);
                resultBitmap.UnlockBits(dstData);

                return resultBitmap;
            }
        }
    }
}
