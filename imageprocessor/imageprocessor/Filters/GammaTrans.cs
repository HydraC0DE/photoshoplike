using System;
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
        public Bitmap ApplyGamma(Bitmap bitmapOriginal, double redGamma, double greenGamma, double blueGamma)
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
    }
}
