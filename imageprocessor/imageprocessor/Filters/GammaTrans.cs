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
      

        public Bitmap ApplyGamma(Bitmap bitmapOriginal, double redGamma, double greenGamma, double blueGamma) //102 ms
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData bmpData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            int stride = bmpData.Stride;
            IntPtr scan0 = bmpData.Scan0;

            // gamma lookuptables
            byte[] redGammaTable = new byte[256];
            byte[] greenGammaTable = new byte[256];
            byte[] blueGammaTable = new byte[256];

            double invBlue = 1 / blueGamma;
            double invGreen = 1/ greenGamma;
            double invRed = 1 / redGamma;

            for (int i = 0; i < 256; ++i) // fill lookup tables
            {
                double normalized = i / 255.0; //small speedup, couldnt find a way around conversions

                redGammaTable[i] = (byte)(Math.Pow(normalized, invRed) * 255.0 + 0.5);
                greenGammaTable[i] = (byte)(Math.Pow(normalized, invGreen) * 255.0 + 0.5);
                blueGammaTable[i] = (byte)(Math.Pow(normalized, invBlue) * 255.0 + 0.5);
            }

            unsafe
            {
                byte* basePtr = (byte*)scan0;

                // partition rows into chunks for parallel threads
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
