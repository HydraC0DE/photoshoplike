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
            // clone the original bitmap so parallel processing is safe later
            Bitmap safeBitmap = (Bitmap)bitmapOriginal.Clone();

            int width = safeBitmap.Width;  // store locally to avoid cross-thread access
            int height = safeBitmap.Height;

            // reduce to 8 bit greyscale image and index pixels
            Bitmap grayBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            ColorPalette palette = grayBitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i); //fill up palette from black to white
            }
            grayBitmap.Palette = palette;

            BitmapData sourceData = safeBitmap.LockBits( //lock the source
                new Rectangle(0, 0, width, height),//entire image
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            BitmapData grayData = grayBitmap.LockBits( //lock the destination
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);

            int srcStride = sourceData.Stride; //byte amount in memory, per row, including padding so we dont index too early 
            int grayStride = grayData.Stride;

            byte[] srcBuffer = new byte[srcStride * height]; //big enough arrays for pixel data
            byte[] grayBuffer = new byte[grayStride * height];

            Marshal.Copy(sourceData.Scan0, srcBuffer, 0, srcBuffer.Length);//pointer to first byte, number of bytes in a row, 0 startindex, length

            Parallel.For(0, height, y => //parallel for to optimize runtime, each thread works on different rows so safew
            {
                int srcOffset = y * srcStride; //offset by padding bytes in memory
                int grayOffset = y * grayStride;
                for (int x = 0; x < width; x++)
                { // b g r order for no reason at all ?
                    byte b = srcBuffer[srcOffset + x * 3]; //get byte values from source
                    byte g = srcBuffer[srcOffset + x * 3 + 1];
                    byte r = srcBuffer[srcOffset + x * 3 + 2];

                    //luminosity formula
                    byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);

                    grayBuffer[grayOffset + x] = gray;
                }
            });

            Marshal.Copy(grayBuffer, 0, grayData.Scan0, grayBuffer.Length);//copy to destination address

            safeBitmap.UnlockBits(sourceData);
            grayBitmap.UnlockBits(grayData);

            safeBitmap.Dispose(); // free cloned bitmap memory

            return grayBitmap;
        }
    }
}
