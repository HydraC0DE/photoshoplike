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
            private readonly float k = 0.04f; // Harris constant
            private readonly float threshold = 1000000f; // Example threshold, tweak as needed
            private readonly int windowRadius = 1; // 3x3 window for non-maximum suppression

            public Bitmap DetectCorners(Bitmap bitmapOriginal) // 1634 ms
            {
                int width = bitmapOriginal.Width;
                int height = bitmapOriginal.Height;

                // 1. Convert to grayscale if needed
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

                unsafe
                {
                    byte* grayPtr = (byte*)grayData.Scan0;
                    byte* cornerPtr = (byte*)cornerData.Scan0;

                    // Allocate arrays for gradient squares
                    float[,] Ix2 = new float[height, width];
                    float[,] Iy2 = new float[height, width];
                    float[,] IxIy = new float[height, width];

                    // 2. Compute gradients and squared products
                    Parallel.For(1, height - 1, y =>
                    {
                        for (int x = 1; x < width - 1; x++)
                        {
                            int idx = y * strideGray + x;
                            int gx = grayPtr[idx + 1] - grayPtr[idx - 1];       // simple horizontal gradient
                            int gy = grayPtr[idx + strideGray] - grayPtr[idx - strideGray]; // simple vertical gradient

                            Ix2[y, x] = gx * gx;
                            Iy2[y, x] = gy * gy;
                            IxIy[y, x] = gx * gy;
                        }
                    });

                    // 3. Compute Harris response R = det(M) - k * trace(M)^2
                    float[,] R = new float[height, width];

                    Parallel.For(1, height - 1, y =>
                    {
                        for (int x = 1; x < width - 1; x++)
                        {
                            // Sum over 3x3 window
                            float sumIx2 = 0, sumIy2 = 0, sumIxIy = 0;
                            for (int wy = -1; wy <= 1; wy++)
                            {
                                for (int wx = -1; wx <= 1; wx++)
                                {
                                    sumIx2 += Ix2[y + wy, x + wx];
                                    sumIy2 += Iy2[y + wy, x + wx];
                                    sumIxIy += IxIy[y + wy, x + wx];
                                }
                            }

                            float det = sumIx2 * sumIy2 - sumIxIy * sumIxIy;
                            float trace = sumIx2 + sumIy2;
                            R[y, x] = det - k * trace * trace;
                        }
                    });

                    // 4. Non-maximum suppression & mark corners
                    Parallel.For(windowRadius, height - windowRadius, y =>
                    {
                        for (int x = windowRadius; x < width - windowRadius; x++)
                        {
                            if (R[y, x] < threshold) continue;

                            bool isMax = true;
                            for (int wy = -windowRadius; wy <= windowRadius && isMax; wy++)
                            {
                                for (int wx = -windowRadius; wx <= windowRadius; wx++)
                                {
                                    if (R[y + wy, x + wx] > R[y, x])
                                    {
                                        isMax = false;
                                        break;
                                    }
                                }
                            }

                            if (isMax)
                            {
                                byte* pixel = cornerPtr + y * strideCorner + x * 3;
                                pixel[0] = 255; // B
                                pixel[1] = 0;   // G
                                pixel[2] = 0;   // R
                            }
                        }
                    });
                }

                grayBitmap.UnlockBits(grayData);
                cornerBitmap.UnlockBits(cornerData);

                if (bitmapOriginal.PixelFormat != PixelFormat.Format8bppIndexed)
                    grayBitmap.Dispose();

                return cornerBitmap;
            }

        public Bitmap DetectCornersFast(Bitmap bitmapOriginal) // 1400
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            // 1. Convert to grayscale if needed
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

                // 2. Compute gradients and squared terms
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

                // 3. Compute Harris response (3×3 window)
                Parallel.For(1, height - 1, y =>
                {
                    int rowOff = y * width;
                    for (int x = 1; x < width - 1; x++)
                    {
                        float sumIx2 = 0f, sumIy2 = 0f, sumIxIy = 0f;
                        int iCenter = rowOff + x;

                        int prevRow = iCenter - width;
                        int nextRow = iCenter + width;

                        // Manual 3×3 sum
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

                // 4. Non-maximum suppression (3×3)
                Parallel.For(1, height - 1, y =>
                {
                    byte* dstRow = cornerPtr + y * strideCorner;
                    int rowOff = y * width;

                    for (int x = 1; x < width - 1; x++)
                    {
                        int i = rowOff + x;
                        float val = R[i];
                        if (val < threshold) continue;

                        // Inline max check (3×3)
                        if (val > R[i - width - 1] && val > R[i - width] && val > R[i - width + 1] &&
                            val > R[i - 1] && val > R[i + 1] &&
                            val > R[i + width - 1] && val > R[i + width] && val > R[i + width + 1])
                        {
                            byte* pixel = dstRow + x * 3;
                            pixel[0] = 255; // Blue
                            pixel[1] = 0;   // Green
                            pixel[2] = 0;   // Red
                        }
                    }
                });
            }

            grayBitmap.UnlockBits(grayData);
            cornerBitmap.UnlockBits(cornerData);

            if (bitmapOriginal.PixelFormat != PixelFormat.Format8bppIndexed)
                grayBitmap.Dispose();

            return cornerBitmap;
        }

    }
}
