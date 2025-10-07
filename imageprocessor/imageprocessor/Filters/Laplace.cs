using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageprocessor.Filters
{
    internal class Laplace
    {
        public Bitmap ApplyLaplace4(Bitmap bitmapOriginal)
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData bitmapData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            int stride = bitmapData.Stride;
            IntPtr Scan0 = bitmapData.Scan0;

            // Create a result bitmap to write to
            Bitmap resultBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData resultData = resultBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* srcBase = (byte*)Scan0;
                byte* dstBase = (byte*)resultData.Scan0;

                Parallel.For(1, height - 1, y =>
                {
                    byte* srcRow = srcBase + (y * stride);
                    byte* dstRow = dstBase + (y * stride);

                    for (int x = 1; x < width - 1; x++)
                    {
                        byte* center = srcRow + x * 3;
                        byte* top = srcRow - stride + x * 3;
                        byte* bottom = srcRow + stride + x * 3;
                        byte* left = center - 3;
                        byte* right = center + 3;

                        for (int c = 0; c < 3; c++) // B, G, R
                        {
                            int newVal =
                                (4 * center[c]) -
                                top[c] -
                                bottom[c] -
                                left[c] -
                                right[c];

                            newVal = Math.Max(0, Math.Min(255, newVal));
                            dstRow[x * 3 + c] = (byte)newVal;
                        }
                    }
                });
            }

            bitmapOriginal.UnlockBits(bitmapData);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }

    }
}
