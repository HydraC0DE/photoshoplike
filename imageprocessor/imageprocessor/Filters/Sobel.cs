using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageprocessor.Filters
{
    internal class Sobel
    {
        public Bitmap ApplySobel(Bitmap bitmapOriginal)
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
                        // We’ll compute Sobel for each color channel separately
                        int gxR = 0, gyR = 0;
                        int gxG = 0, gyG = 0;
                        int gxB = 0, gyB = 0;

                        // Precompute pixel pointers for 3x3 neighborhood
                        byte* p00 = srcBase + (y - 1) * stride + (x - 1) * 3;
                        byte* p01 = srcBase + (y - 1) * stride + (x) * 3;
                        byte* p02 = srcBase + (y - 1) * stride + (x + 1) * 3;

                        byte* p10 = srcBase + (y) * stride + (x - 1) * 3;
                        byte* p11 = srcBase + (y) * stride + (x) * 3;
                        byte* p12 = srcBase + (y) * stride + (x + 1) * 3;

                        byte* p20 = srcBase + (y + 1) * stride + (x - 1) * 3;
                        byte* p21 = srcBase + (y + 1) * stride + (x) * 3;
                        byte* p22 = srcBase + (y + 1) * stride + (x + 1) * 3;

                        // Sobel Gx kernel:
                        // [-1 0 +1]
                        // [-2 0 +2]
                        // [-1 0 +1]

                        // Sobel Gy kernel:
                        // [-1 -2 -1]
                        // [ 0  0  0]
                        // [+1 +2 +1]

                        // Compute Gx and Gy for each color channel
                        for (int c = 0; c < 3; c++)
                        {
                            gxB += (-p00[c] - 2 * p10[c] - p20[c] + p02[c] + 2 * p12[c] + p22[c]);
                            gyB += (-p00[c] - 2 * p01[c] - p02[c] + p20[c] + 2 * p21[c] + p22[c]);
                        }

                        // Separate color channels explicitly (R,G,B order in memory is B,G,R)
                        gxB = (-p00[0] - 2 * p10[0] - p20[0] + p02[0] + 2 * p12[0] + p22[0]);
                        gxG = (-p00[1] - 2 * p10[1] - p20[1] + p02[1] + 2 * p12[1] + p22[1]);
                        gxR = (-p00[2] - 2 * p10[2] - p20[2] + p02[2] + 2 * p12[2] + p22[2]);

                        gyB = (-p00[0] - 2 * p01[0] - p02[0] + p20[0] + 2 * p21[0] + p22[0]);
                        gyG = (-p00[1] - 2 * p01[1] - p02[1] + p20[1] + 2 * p21[1] + p22[1]);
                        gyR = (-p00[2] - 2 * p01[2] - p02[2] + p20[2] + 2 * p21[2] + p22[2]);

                        // Approximate magnitude (faster than sqrt)
                        int valR = Math.Min(255, Math.Abs(gxR) + Math.Abs(gyR));
                        int valG = Math.Min(255, Math.Abs(gxG) + Math.Abs(gyG));
                        int valB = Math.Min(255, Math.Abs(gxB) + Math.Abs(gyB));

                        byte* dstPixel = dstRow + x * 3;
                        dstPixel[0] = (byte)valB;
                        dstPixel[1] = (byte)valG;
                        dstPixel[2] = (byte)valR;
                    }
                });
            }

            bitmapOriginal.UnlockBits(srcData);
            resultBitmap.UnlockBits(dstData);

            return resultBitmap;
        }


    }
}
