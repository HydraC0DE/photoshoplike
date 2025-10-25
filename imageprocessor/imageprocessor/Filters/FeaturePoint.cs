using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageprocessor.Filters
{
    internal class FeaturePoint
    {
            private readonly float k = 0.04f; // harris constant
            private readonly float threshold = 1000000f; // threshhold
            private readonly int windowRadius = 1; // 3x3 window for non-maximum suppression

        public Bitmap DetectCornersFast(Bitmap bitmapOriginal) // 687 ms
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            // convert to grayscale if needed
            Bitmap grayBitmap = bitmapOriginal.PixelFormat == PixelFormat.Format8bppIndexed
                ? bitmapOriginal
                : new Grayscale().Apply8BitImage(bitmapOriginal);

            BitmapData grayData = grayBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format8bppIndexed);

            Bitmap cornerBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData cornerData = cornerBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            int strideGray = grayData.Stride;
            int strideCorner = cornerData.Stride;
            int size = width * height;

            float[] Ix2 = new float[size];
            float[] Iy2 = new float[size];
            float[] IxIy = new float[size];
            float[] R = new float[size];

            unsafe
            {
                byte* grayPtr = (byte*)grayData.Scan0;
                byte* cornerPtr = (byte*)cornerData.Scan0;

                // compute gradients and squared
                Parallel.For(1, height - 1, y =>
                {
                    int yOff = y * strideGray;
                    int prevOff = (y - 1) * strideGray;
                    int nextOff = (y + 1) * strideGray;
                    int rowOff = y * width;

                    for (int x = 1; x < width - 1; x++)
                    {
                        int idxGray = yOff + x;
                        int gx = grayPtr[idxGray + 1] - grayPtr[idxGray - 1];
                        int gy = grayPtr[nextOff + x] - grayPtr[prevOff + x];

                        int i = rowOff + x;
                        float fx = gx;
                        float fy = gy;

                        Ix2[i] = fx * fx;
                        Iy2[i] = fy * fy;
                        IxIy[i] = fx * fy;
                    }
                });

                // compute harris
                Parallel.For(1, height - 1, y =>
                {
                    int rowOff = y * width;
                    for (int x = 1; x < width - 1; x++)
                    {
                        float sumIx2 = 0f, sumIy2 = 0f, sumIxIy = 0f;
                        int iCenter = rowOff + x;

                        int prevRow = iCenter - width;
                        int nextRow = iCenter + width;

                        // manually
                        sumIx2 =
                            Ix2[prevRow - 1] + Ix2[prevRow] + Ix2[prevRow + 1] +
                            Ix2[iCenter - 1] + Ix2[iCenter] + Ix2[iCenter + 1] +
                            Ix2[nextRow - 1] + Ix2[nextRow] + Ix2[nextRow + 1];

                        sumIy2 =
                            Iy2[prevRow - 1] + Iy2[prevRow] + Iy2[prevRow + 1] +
                            Iy2[iCenter - 1] + Iy2[iCenter] + Iy2[iCenter + 1] +
                            Iy2[nextRow - 1] + Iy2[nextRow] + Iy2[nextRow + 1];

                        sumIxIy =
                            IxIy[prevRow - 1] + IxIy[prevRow] + IxIy[prevRow + 1] +
                            IxIy[iCenter - 1] + IxIy[iCenter] + IxIy[iCenter + 1] +
                            IxIy[nextRow - 1] + IxIy[nextRow] + IxIy[nextRow + 1];

                        float det = sumIx2 * sumIy2 - sumIxIy * sumIxIy;
                        float trace = sumIx2 + sumIy2;
                        R[iCenter] = det - k * trace * trace;
                    }
                });

                // non maximum
                Parallel.For(1, height - 1, y =>
                {
                    byte* dstRow = cornerPtr + y * strideCorner;
                    int rowOff = y * width;

                    for (int x = 1; x < width - 1; x++)
                    {
                        int i = rowOff + x;
                        float val = R[i];
                        if (val < threshold) continue;

                        // max check
                        if (val > R[i - width - 1] && val > R[i - width] && val > R[i - width + 1] &&
                            val > R[i - 1] && val > R[i + 1] &&
                            val > R[i + width - 1] && val > R[i + width] && val > R[i + width + 1])
                        {
                            byte* pixel = dstRow + x * 3;
                            pixel[0] = 255; // b, make the feature points blue everything else black, if wanted to keep orignal then ony this line, could be faster like that
                            pixel[1] = 0;   // g
                            pixel[2] = 0;   // r
                        }
                    }
                });
            }

            grayBitmap.UnlockBits(grayData);
            cornerBitmap.UnlockBits(cornerData);

            if (bitmapOriginal.PixelFormat != PixelFormat.Format8bppIndexed) //if not 8bit
            {
                grayBitmap.Dispose();
            }

            return cornerBitmap;
        }

    }
}
