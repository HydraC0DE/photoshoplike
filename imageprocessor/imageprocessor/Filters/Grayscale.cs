using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace imageprocessor.Filters
{
    internal class Grayscale
    {
        public Bitmap Apply8BitImage(Bitmap bitmapOriginal) //new image with 8bitdepth grayscale 
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            // New 8-bit grayscale bitmap
            Bitmap bmp8 = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            // Set grayscale palette
            ColorPalette palette = bmp8.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i); //fill up 256 shades of gray

            }

            bmp8.Palette = palette;

            // Lock source and destination
            BitmapData srcData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            BitmapData dstData = bmp8.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);

            int srcStride = srcData.Stride; // number of bytes per row in source
            int dstStride = dstData.Stride; // number of bytes per row in destination

            IntPtr srcScan0 = srcData.Scan0; // pointer to the first pixel of source
            IntPtr dstScan0 = dstData.Scan0; // pointer to the first pixel of destination

            const int rFactor = 77;
            const int gFactor = 150;
            const int bFactor = 29;

            unsafe
            {
                byte* srcBase = (byte*)srcScan0;
                byte* dstBase = (byte*)dstScan0;

                var range = Partitioner.Create(0, height, Math.Max(1, height / Environment.ProcessorCount)); // pointer to the first pixel of destination
                Parallel.ForEach(range, (rangeSegment) =>
                {
                    for (int y = rangeSegment.Item1; y < rangeSegment.Item2; y++)
                    {
                        byte* srcRow = srcBase + y * srcStride;
                        byte* dstRow = dstBase + y * dstStride;
                        int x = 0;

                        int maxUnroll = width - 3;
                        for (; x < maxUnroll; x += 4)
                        {
                            // pixel 0
                            {
                                byte b = srcRow[0];
                                byte g = srcRow[1];
                                byte r = srcRow[2];
                                byte gray = (byte)((r * rFactor + g * gFactor + b * bFactor) >> 8); // fast intiger division by 256
                                dstRow[0] = gray;
                                srcRow += 3;
                                dstRow += 1;
                            }
                            // pixel 1
                            {
                                byte b = srcRow[0];
                                byte g = srcRow[1];
                                byte r = srcRow[2];
                                byte gray = (byte)((r * rFactor + g * gFactor + b * bFactor) >> 8);
                                dstRow[0] = gray;
                                srcRow += 3;
                                dstRow += 1;
                            }
                            // pixel 2
                            {
                                byte b = srcRow[0];
                                byte g = srcRow[1];
                                byte r = srcRow[2];
                                byte gray = (byte)((r * rFactor + g * gFactor + b * bFactor) >> 8);
                                dstRow[0] = gray;
                                srcRow += 3;
                                dstRow += 1;
                            }
                            // pixel 3
                            {
                                byte b = srcRow[0];
                                byte g = srcRow[1];
                                byte r = srcRow[2];
                                byte gray = (byte)((r * rFactor + g * gFactor + b * bFactor) >> 8);
                                dstRow[0] = gray;
                                srcRow += 3;
                                dstRow += 1;
                            }
                        }

                        // remaining pixels
                        for (; x < width; x++)
                        {
                            byte b = srcRow[0];
                            byte g = srcRow[1];
                            byte r = srcRow[2];
                            byte gray = (byte)((r * rFactor + g * gFactor + b * bFactor) >> 8);
                            dstRow[0] = gray;
                            srcRow += 3;
                            dstRow += 1;
                        }
                    }
                });
            }

            bitmapOriginal.UnlockBits(srcData);
            bmp8.UnlockBits(dstData);

            return bmp8;
        }


        public Bitmap FastApply(Bitmap bitmapOriginal) //91 ms
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData bmpData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            int stride = bmpData.Stride;
            IntPtr scan0 = bmpData.Scan0;

            // this was used in official libraries
            const int rFactor = 77;   // 0.299 × 256
            const int gFactor = 150;  // 0.587 × 256
            const int bFactor = 29;   // 0.114 × 256

            unsafe
            {
                byte* basePtr = (byte*)scan0;

                // partition rows
                var range = Partitioner.Create(0, height, Math.Max(1, height / Environment.ProcessorCount)); //devide into chunks (tuples) roughly height / number of cores
                Parallel.ForEach(range, (rangeSegment) =>
                {
                    for (int y = rangeSegment.Item1; y < rangeSegment.Item2; y++)
                    {
                        byte* row = basePtr + y * stride;
                        int x = 0;

                        // process 4 pixels per iteration as unrolling
                        int maxUnroll = width - 3;
                        for (; x < maxUnroll; x += 4)
                        {
                            // pixel 0
                            {
                                byte b = row[0];
                                byte g = row[1];
                                byte r = row[2];
                                byte gray = (byte)((r * rFactor + g * gFactor + b * bFactor) >> 8);
                                row[0] = gray;
                                row[1] = gray;
                                row[2] = gray;
                                row += 3;
                            }
                            // pixel 1
                            {
                                byte b = row[0];
                                byte g = row[1];
                                byte r = row[2];
                                byte gray = (byte)((r * rFactor + g * gFactor + b * bFactor) >> 8);
                                row[0] = gray;
                                row[1] = gray;
                                row[2] = gray;
                                row += 3;
                            }
                            // pixel 2
                            {
                                byte b = row[0];
                                byte g = row[1];
                                byte r = row[2];
                                byte gray = (byte)((r * rFactor + g * gFactor + b * bFactor) >> 8);
                                row[0] = gray;
                                row[1] = gray;
                                row[2] = gray;
                                row += 3;
                            }
                            // pixel 3
                            {
                                byte b = row[0];
                                byte g = row[1];
                                byte r = row[2];
                                byte gray = (byte)((r * rFactor + g * gFactor + b * bFactor) >> 8);
                                row[0] = gray;
                                row[1] = gray;
                                row[2] = gray;
                                row += 3;
                            }
                        }

                        // remaining pixels if not divisible by 4
                        for (; x < width; x++)
                        {
                            byte b = row[0];
                            byte g = row[1];
                            byte r = row[2];
                            byte gray = (byte)((r * rFactor + g * gFactor + b * bFactor) >> 8);
                            row[0] = gray;
                            row[1] = gray;
                            row[2] = gray;
                            row += 3;
                        }
                    }
                });
            }

            bitmapOriginal.UnlockBits(bmpData);
            return bitmapOriginal;
        }


        
    }
}
