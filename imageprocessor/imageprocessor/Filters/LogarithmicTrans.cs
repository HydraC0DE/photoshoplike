using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageprocessor.Filters
{
    public class LogarithmicTrans
    {
        public Bitmap ApplyLogartihmic(Bitmap bitmapOriginal, double normalizationFactor = -1)
        {
            if (normalizationFactor == -1 || normalizationFactor<0) //user didn't specify value so lets go with 46 which is apperently good for this
            {
                normalizationFactor = normalizationFactor = 255.0 / Math.Log(1 + 255);
            }
            //should be between 1 and 100, will specify this in UI

            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData bitmapData = bitmapOriginal.LockBits(
        new Rectangle(0, 0, width, height),
        ImageLockMode.ReadWrite,
        PixelFormat.Format24bppRgb);

            int stride = bitmapData.Stride;
            IntPtr Scan0 = bitmapData.Scan0;

            byte[] logTable = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                logTable[i] = (byte)Math.Min(255, normalizationFactor * Math.Log(1 + i));
            }

          

            unsafe
            {
                byte* basePointer = (byte*)Scan0;
                Parallel.For(0, height, y =>
                {
                    byte* row = basePointer + (y * stride); // pointer to start of row

                    for (int x = 0; x < width; x++)
                    {
                        row[0] = logTable[row[0]];
                        row[1] = logTable[row[1]];
                        row[2] = logTable[row[2]];
                        

                        row += 3; // move to next pixel
                    }
                });
            }

            bitmapOriginal.UnlockBits(bitmapData);
            return bitmapOriginal;
        }
    }
}
