using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace imageprocessor.Filters
{
    internal class Grayscale
    {
        public Bitmap Apply8BitImage(Bitmap bitmapOriginal) //new image with 8bitdepth grayscale 
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            // reduce to 8 bit greyscale image and index pixels
            Bitmap grayBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            ColorPalette palette = grayBitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i); //fill up palette from black to white
            }
            grayBitmap.Palette = palette;

            BitmapData sourceData = bitmapOriginal.LockBits( //lock the source
                new Rectangle(0, 0, width, height),//entire image
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            BitmapData grayData = grayBitmap.LockBits( //lock the destination
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);

            int srcStride = sourceData.Stride; //byte amount in memory, per row, including padding so we dont index too early 
            int grayStride = grayData.Stride;

            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0; // pointer to first source pixel
                byte* grayPtr = (byte*)grayData.Scan0;  // pointer to first dest pixel

                Parallel.For(0, height, y => //parallel for to optimize runtime, each thread works on different rows so safew
                {

                    byte* srcRow = srcPtr + (y * srcStride);   // start of current row in source
                    byte* grayRow = grayPtr + (y * grayStride); // start of current row in dest

               
                    for (int x = 0; x < width; x++)
                    { // b g r order for no reason at all ?
                        byte b = srcRow[x * 3]; //get byte values from source
                        byte g = srcRow[x * 3 + 1];
                        byte r = srcRow[x * 3 + 2];

                        //luminosity formula
                        byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);

                        grayRow[x] = gray;
                    }
                });
            }

            bitmapOriginal.UnlockBits(sourceData);
            grayBitmap.UnlockBits(grayData);

            return grayBitmap;
        }

        public Bitmap FastApply(Bitmap bitmapOriginal)
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData bitmapData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, bitmapOriginal.Width, bitmapOriginal.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format24bppRgb);

            int stride = bitmapData.Stride; //byte amount in memory, per row, including padding so we dont index too early 
            IntPtr Scan0 = bitmapData.Scan0; //first byte pointer

            unsafe
            {
                byte* basePointer = (byte*)Scan0;

                Parallel.For(0, height, y =>
                {
                    byte* row = basePointer + (y * stride); // pointer to start of row

                    for (int x = 0; x < width; ++x)//increase value before using it
                    {
                        byte b = row[0];
                        byte g = row[1];
                        byte r = row[2];

                        // grayscale formula
                        byte gray = (byte)(.299 * r + .587 * g + .114 * b);

                        row[0] = row[1] = row[2] = gray;

                        row += 3; // move to next pixel
                    }
                });
            }

            bitmapOriginal.UnlockBits(bitmapData);
            return bitmapOriginal;
        }
    }
}
