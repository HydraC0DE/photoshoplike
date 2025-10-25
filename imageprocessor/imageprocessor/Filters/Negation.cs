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


        public Bitmap Negate(Bitmap bitmapOriginal) //73 ms
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData bmpData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            int stride = bmpData.Stride; //number of bytes per row /w padding
            IntPtr scan0 = bmpData.Scan0; //pointer to first pixel

            unsafe
            {
                byte* basePtr = (byte*)scan0; //base pointer to pixel data

                // partition for multiple threads
                var range = Partitioner.Create(0, height, Math.Max(1, height / Environment.ProcessorCount));

                Parallel.ForEach(range, (rangeSegment) =>
                {
                    for (int y = rangeSegment.Item1; y < rangeSegment.Item2; y++)
                    {
                        byte* row = basePtr + y * stride;
                        int x = 0;

                        // process 4 pixels per iteration
                        int maxUnroll = width - 3;
                        for (x = x; x < maxUnroll; x += 4)
                        {
                            // pixel 0
                            row[0] = (byte)(255 - row[0]); //faster then lookup even with conversion
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
