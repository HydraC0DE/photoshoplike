using System;
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
        public Bitmap ApplyBoxFilter(Bitmap bitmapOriginal)
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

    }
}
