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
    public class Negation
    {
        public Bitmap NegateOld(Bitmap bitmapOriginal)//100 ms
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData bitmapData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, bitmapOriginal.Width, bitmapOriginal.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            int stride = bitmapData.Stride;
            IntPtr Scan0 = bitmapData.Scan0;

            unsafe
            {
                byte* basePointer = (byte*)Scan0;

                Parallel.For(0, height, y =>
                {
                    byte* row = basePointer + (y * stride); // pointer to start of row

                    for (int x = 0; x < width * 3; x++)
                    {
                        row[x] = (byte)(255 - row[x]); //negation formula
                    }
                });
            }

            bitmapOriginal.UnlockBits(bitmapData);
            return bitmapOriginal;
        }

        public Bitmap Negate(Bitmap bitmapOriginal) //73 ms
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData bmpData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            int stride = bmpData.Stride;
            IntPtr scan0 = bmpData.Scan0;

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
                            row[0] = (byte)(255 - row[0]);
                            row[1] = (byte)(255 - row[1]);
                            row[2] = (byte)(255 - row[2]);
                            row += 3;

                            // pixel 1
                            row[0] = (byte)(255 - row[0]);
                            row[1] = (byte)(255 - row[1]);
                            row[2] = (byte)(255 - row[2]);
                            row += 3;

                            // pixel 2
                            row[0] = (byte)(255 - row[0]);
                            row[1] = (byte)(255 - row[1]);
                            row[2] = (byte)(255 - row[2]);
                            row += 3;

                            // pixel 3
                            row[0] = (byte)(255 - row[0]);
                            row[1] = (byte)(255 - row[1]);
                            row[2] = (byte)(255 - row[2]);
                            row += 3;
                        }

                        // Remaining pixels
                        for (; x < width; x++)
                        {
                            row[0] = (byte)(255 - row[0]);
                            row[1] = (byte)(255 - row[1]);
                            row[2] = (byte)(255 - row[2]);
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
