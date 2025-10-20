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
    internal class BoxFilter
    {
        public Bitmap ApplyBoxFilter(Bitmap bitmapOriginal) //458
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
                        int sumR = 0, sumG = 0, sumB = 0;

                        // Loop through 3x3 neighborhood
                        for (int ky = -1; ky <= 1; ky++)
                        {
                            byte* srcRow = srcBase + (y + ky) * stride;
                            for (int kx = -1; kx <= 1; kx++)
                            {
                                byte* pixel = srcRow + (x + kx) * 3;
                                sumB += pixel[0];
                                sumG += pixel[1];
                                sumR += pixel[2];
                            }
                        }

                        byte* dstPixel = dstRow + x * 3;
                        dstPixel[0] = (byte)(sumB / 9);
                        dstPixel[1] = (byte)(sumG / 9);
                        dstPixel[2] = (byte)(sumR / 9);
                    }
                });
            }

            bitmapOriginal.UnlockBits(srcData);
            resultBitmap.UnlockBits(dstData);

            return resultBitmap;
        }

        public Bitmap ApplyBoxFilter_Fast(Bitmap bitmapOriginal)//229
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

                // Divide rows among threads efficiently
                var range = Partitioner.Create(1, height - 1, Math.Max(1, height / Environment.ProcessorCount));
                Parallel.ForEach(range, (rangeSegment) =>
                {
                    for (int y = rangeSegment.Item1; y < rangeSegment.Item2; y++)
                    {
                        byte* prevRow = srcBase + (y - 1) * stride;
                        byte* curRow = srcBase + y * stride;
                        byte* nextRow = srcBase + (y + 1) * stride;
                        byte* dstRow = dstBase + y * stride;

                        for (int x = 1; x < width - 1; x++)
                        {
                            int idx = x * 3;

                            // Preload pixel neighborhoods (3x3 window)
                            byte* p00 = prevRow + (idx - 3);
                            byte* p01 = prevRow + idx;
                            byte* p02 = prevRow + (idx + 3);

                            byte* p10 = curRow + (idx - 3);
                            byte* p11 = curRow + idx;
                            byte* p12 = curRow + (idx + 3);

                            byte* p20 = nextRow + (idx - 3);
                            byte* p21 = nextRow + idx;
                            byte* p22 = nextRow + (idx + 3);

                            // Sum up all 9 pixels directly
                            int sumB =
                                p00[0] + p01[0] + p02[0] +
                                p10[0] + p11[0] + p12[0] +
                                p20[0] + p21[0] + p22[0];

                            int sumG =
                                p00[1] + p01[1] + p02[1] +
                                p10[1] + p11[1] + p12[1] +
                                p20[1] + p21[1] + p22[1];

                            int sumR =
                                p00[2] + p01[2] + p02[2] +
                                p10[2] + p11[2] + p12[2] +
                                p20[2] + p21[2] + p22[2];

                            byte* dstPixel = dstRow + idx;
                            dstPixel[0] = (byte)(sumB / 9);
                            dstPixel[1] = (byte)(sumG / 9);
                            dstPixel[2] = (byte)(sumR / 9);
                        }

                        // Set borders to 0 for consistency
                        dstRow[0] = dstRow[1] = dstRow[2] = 0;
                        int rb = (width - 1) * 3;
                        dstRow[rb + 0] = dstRow[rb + 1] = dstRow[rb + 2] = 0;
                    }
                });
            }

            bitmapOriginal.UnlockBits(srcData);
            resultBitmap.UnlockBits(dstData);

            return resultBitmap;
        }

    }
}
