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
        
        public Bitmap ApplyLaplace(Bitmap bitmapOriginal) //242 ms
        {
            // 0 -1 0
            // -1 4 -1
            // 0 -1 0
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            // lock source bitmap for reading pixel data
            BitmapData srcData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;   // number of bytes per row (including padding)
            IntPtr srcScan0 = srcData.Scan0; // pointer to first pixel in memory

            // create output bitmap and lock it for writing
            Bitmap resultBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData dstData = resultBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* srcBase = (byte*)srcScan0; // base pointer to input pixels
                byte* dstBase = (byte*)dstData.Scan0; // base pointer to output pixels

                // process image rows in parallel (excluding top and bottom borders)
                Parallel.For(1, height - 1, y =>
                {
                    // get pointers to the three neighboring rows
                    byte* prevRow = srcBase + (y - 1) * stride; // row above
                    byte* curRow = srcBase + y * stride;        // current row
                    byte* nextRow = srcBase + (y + 1) * stride; // row below

                    // pointer to the destination row
                    byte* outRow = dstBase + y * stride;

                    // define horizontal processing limits (skip first and last column)
                    int rightBound = width - 1;
                    int w = width;

                    // process each pixel except image borders
                    for (int x = 1; x < rightBound; x++)
                    {
                        int baseIdx = x * 3;  // pixel start index in row (3 bytes per pixel)
                        int leftIdx = baseIdx - 3;  // left neighbor index
                        int rightIdx = baseIdx + 3; // right neighbor index

                        // blue channel
                        {
                            int center = curRow[baseIdx + 0];
                            int top = prevRow[baseIdx + 0];
                            int bottom = nextRow[baseIdx + 0];
                            int left = curRow[leftIdx + 0];
                            int right = curRow[rightIdx + 0];

                            int val = (4 * center) - top - bottom - left - right; // laplace 4-neighbor filter
                            if (val < 0) val = 0;
                            else if (val > 255) val = 255;
                            outRow[baseIdx + 0] = (byte)val;
                        }

                        // green channel
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

                        // red channel
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

                    // set left and right border pixels to zero but its not noticable so its commented
                    //outRow[0] = outRow[1] = outRow[2] = 0; // left edge
                    //int rb = (w - 1) * 3;
                    //outRow[rb + 0] = outRow[rb + 1] = outRow[rb + 2] = 0; // right edge
                });
            }

            // unlock both bitmaps to apply changes
            bitmapOriginal.UnlockBits(srcData);
            resultBitmap.UnlockBits(dstData);

            return resultBitmap; // return the filtered image
        }



    }

}
