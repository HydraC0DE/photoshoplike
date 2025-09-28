using System;
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
        public Bitmap Negate(Bitmap bitmapOriginal)
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
    }
}
