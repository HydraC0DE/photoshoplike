using System;
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
        public Bitmap ApplyGaussianFilterFast(Bitmap bitmapOriginal)
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
                        // Pointers to the 3 rows around the pixel
                        byte* rowAbove = srcBase + (y - 1) * stride + (x - 1) * 3;
                        byte* rowCurrent = srcBase + y * stride + (x - 1) * 3;
                        byte* rowBelow = srcBase + (y + 1) * stride + (x - 1) * 3;

                        // Blue channel
                        int sumB =
                            rowAbove[0] + 2 * rowAbove[3] + rowAbove[6] +
                            2 * rowCurrent[0] + 4 * rowCurrent[3] + 2 * rowCurrent[6] +
                            rowBelow[0] + 2 * rowBelow[3] + rowBelow[6];

                        // Green channel
                        int sumG =
                            rowAbove[1] + 2 * rowAbove[4] + rowAbove[7] +
                            2 * rowCurrent[1] + 4 * rowCurrent[4] + 2 * rowCurrent[7] +
                            rowBelow[1] + 2 * rowBelow[4] + rowBelow[7];

                        // Red channel
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

    }
}
