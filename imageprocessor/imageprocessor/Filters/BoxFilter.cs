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

                // get chunks again
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

                            // pre-do the box filter
                            byte* p00 = prevRow + (idx - 3);
                            byte* p01 = prevRow + idx;
                            byte* p02 = prevRow + (idx + 3);

                            byte* p10 = curRow + (idx - 3);
                            byte* p11 = curRow + idx;
                            byte* p12 = curRow + (idx + 3);

                            byte* p20 = nextRow + (idx - 3);
                            byte* p21 = nextRow + idx;
                            byte* p22 = nextRow + (idx + 3);

                            // sum
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

                            byte* dstPixel = dstRow + idx; //divide by nine and
                            dstPixel[0] = (byte)(sumB / 9);
                            dstPixel[1] = (byte)(sumG / 9);
                            dstPixel[2] = (byte)(sumR / 9);
                        }

                        // set borders to 0 for consistency, irrelevant again
                        //dstRow[0] = dstRow[1] = dstRow[2] = 0;
                        //int rb = (width - 1) * 3;
                        //dstRow[rb + 0] = dstRow[rb + 1] = dstRow[rb + 2] = 0;
                    }
                });
            }

            bitmapOriginal.UnlockBits(srcData);
            resultBitmap.UnlockBits(dstData);

            return resultBitmap;
        }

    }
}
