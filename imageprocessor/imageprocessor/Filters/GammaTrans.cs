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
    public class GammaTrans
    {
        public Bitmap ApplyGammaOld(Bitmap bitmapOriginal, double redGamma, double greenGamma, double blueGamma) //122
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData bitmapData = bitmapOriginal.LockBits(
        new Rectangle(0, 0, width, height),
        ImageLockMode.ReadWrite,
        PixelFormat.Format24bppRgb);

            int stride = bitmapData.Stride;
            IntPtr Scan0 = bitmapData.Scan0;

            byte[] redGammaTable = new byte[256];
            byte[] greenGammaTable = new byte[256];
            byte[] blueGammaTable = new byte[256];
            
            for (int i = 0; i < 256; ++i)
            {
                redGammaTable[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / redGamma)) + 0.5));
                greenGammaTable[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / greenGamma)) + 0.5));
                blueGammaTable[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / blueGamma)) + 0.5));
            }
            unsafe
            {
                byte* basePointer = (byte*)Scan0;
                Parallel.For(0, height, y =>
                {
                    byte* row = basePointer + (y * stride); // pointer to start of row

                    for (int x = 0; x < width; x++)
                    {
                        row[0] = blueGammaTable[row[0]]; //b
                        row[1] = greenGammaTable[row[1]];   // g
                        row[2] = redGammaTable[row[2]]; // r

                        row += 3; // move to next pixel
                    }
                });
            }

            bitmapOriginal.UnlockBits(bitmapData);
            return bitmapOriginal;
        }

        public Bitmap ApplyGamma(Bitmap bitmapOriginal, double redGamma, double greenGamma, double blueGamma) //102
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData bmpData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            int stride = bmpData.Stride;
            IntPtr scan0 = bmpData.Scan0;

            // Precompute gamma lookup tables
            byte[] redGammaTable = new byte[256];
            byte[] greenGammaTable = new byte[256];
            byte[] blueGammaTable = new byte[256];

            double invBlue = 1 / blueGamma;
            double invGreen = 1/ greenGamma;
            double invRed = 1 / redGamma;

            for (int i = 0; i < 256; ++i)
            {
                double normalized = i / 255.0;

                redGammaTable[i] = (byte)(Math.Pow(normalized, invRed) * 255.0 + 0.5);
                greenGammaTable[i] = (byte)(Math.Pow(normalized, invGreen) * 255.0 + 0.5);
                blueGammaTable[i] = (byte)(Math.Pow(normalized, invBlue) * 255.0 + 0.5);
            }

            unsafe
            {
                byte* basePtr = (byte*)scan0;

                // Partition rows into chunks for parallel threads
                var range = Partitioner.Create(0, height, Math.Max(1, height / Environment.ProcessorCount));
                Parallel.ForEach(range, (rangeSegment) =>
                {
                    for (int y = rangeSegment.Item1; y < rangeSegment.Item2; y++)
                    {
                        byte* row = basePtr + y * stride;
                        int x = 0;

                        // Process 4 pixels per iteration (loop unrolling)
                        int maxUnroll = width - 3;
                        for (; x < maxUnroll; x += 4)
                        {
                            // pixel 0
                            row[0] = blueGammaTable[row[0]];
                            row[1] = greenGammaTable[row[1]];
                            row[2] = redGammaTable[row[2]];
                            row += 3;

                            // pixel 1
                            row[0] = blueGammaTable[row[0]];
                            row[1] = greenGammaTable[row[1]];
                            row[2] = redGammaTable[row[2]];
                            row += 3;

                            // pixel 2
                            row[0] = blueGammaTable[row[0]];
                            row[1] = greenGammaTable[row[1]];
                            row[2] = redGammaTable[row[2]];
                            row += 3;

                            // pixel 3
                            row[0] = blueGammaTable[row[0]];
                            row[1] = greenGammaTable[row[1]];
                            row[2] = redGammaTable[row[2]];
                            row += 3;
                        }

                        // Remaining pixels
                        for (; x < width; x++)
                        {
                            row[0] = blueGammaTable[row[0]];
                            row[1] = greenGammaTable[row[1]];
                            row[2] = redGammaTable[row[2]];
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
