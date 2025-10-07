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

            public Bitmap DetectCorners(Bitmap bitmapOriginal)
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
        }
}
