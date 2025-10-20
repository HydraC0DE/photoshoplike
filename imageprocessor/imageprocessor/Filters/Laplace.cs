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
    internal class Laplace
    {
        public Bitmap ApplyLaplace4Old(Bitmap bitmapOriginal) // 300 ms
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

        public Bitmap ApplyLaplace4_CacheFriendly(Bitmap bitmapOriginal) //242
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData srcData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;
            IntPtr srcScan0 = srcData.Scan0;

            // Create output bitmap (same pixel format)
            Bitmap resultBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData dstData = resultBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* srcBase = (byte*)srcScan0;
                byte* dstBase = (byte*)dstData.Scan0;

                // Process rows 1 .. height-2 (skip image borders)
                Parallel.For(1, height - 1, y =>
                {
                    // Pointers to the three rows we'll read from
                    byte* prevRow = srcBase + (y - 1) * stride;
                    byte* curRow = srcBase + (y) * stride;
                    byte* nextRow = srcBase + (y + 1) * stride;

                    // Destination row pointer
                    byte* outRow = dstBase + y * stride;

                    // We will iterate x = 1 .. width-2 (skip border columns)
                    int rightBound = width - 1;

                    // Use local variables to avoid repeated capture
                    int w = width;

                    for (int x = 1; x < rightBound; x++)
                    {
                        int baseIdx = x * 3; // index into the row for channel 0 (B)

                        // Read neighbors and center for the three channels
                        // Access pattern: prevRow[baseIdx + c], curRow[baseIdx + c], nextRow[baseIdx + c]
                        // and left: curRow[baseIdx - 3], right: curRow[baseIdx + 3]
                        int leftIdx = baseIdx - 3;
                        int rightIdx = baseIdx + 3;

                        // Blue channel (c = 0)
                        {
                            int center = curRow[baseIdx + 0];
                            int top = prevRow[baseIdx + 0];
                            int bottom = nextRow[baseIdx + 0];
                            int left = curRow[leftIdx + 0];
                            int right = curRow[rightIdx + 0];

                            int val = (4 * center) - top - bottom - left - right;
                            if (val < 0) val = 0;
                            else if (val > 255) val = 255;
                            outRow[baseIdx + 0] = (byte)val;
                        }

                        // Green channel (c = 1)
                        {
                            int center = curRow[baseIdx + 1];
                            int top = prevRow[baseIdx + 1];
                            int bottom = nextRow[baseIdx + 1];
                            int left = curRow[leftIdx + 1];
                            int right = curRow[rightIdx + 1];

                            int val = (4 * center) - top - bottom - left - right;
                            if (val < 0) val = 0;
                            else if (val > 255) val = 255;
                            outRow[baseIdx + 1] = (byte)val;
                        }

                        // Red channel (c = 2)
                        {
                            int center = curRow[baseIdx + 2];
                            int top = prevRow[baseIdx + 2];
                            int bottom = nextRow[baseIdx + 2];
                            int left = curRow[leftIdx + 2];
                            int right = curRow[rightIdx + 2];

                            int val = (4 * center) - top - bottom - left - right;
                            if (val < 0) val = 0;
                            else if (val > 255) val = 255;
                            outRow[baseIdx + 2] = (byte)val;
                        }
                    }

                    // set border pixels to zero
                    // Left border (x=0)
                    outRow[0] = outRow[1] = outRow[2] = 0;
                    // Right border (x = width-1)
                    int rb = (w - 1) * 3;
                    outRow[rb + 0] = outRow[rb + 1] = outRow[rb + 2] = 0;
                });
            }

            bitmapOriginal.UnlockBits(srcData);
            resultBitmap.UnlockBits(dstData);

            return resultBitmap;
        }


    }

}
