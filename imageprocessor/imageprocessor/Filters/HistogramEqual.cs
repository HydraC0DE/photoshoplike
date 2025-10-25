using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageprocessor.Filters
{
    public class HistogramEqualization
    {
        public Bitmap HistogramEqualize(Bitmap b) //268
        {
            int width = b.Width;
            int height = b.Height;


            Bitmap result = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            BitmapData srcData = b.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);


            BitmapData dstData = result.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int strideSrc = srcData.Stride;
            int strideDst = dstData.Stride;

            IntPtr scan0Src = srcData.Scan0;
            IntPtr scan0Dst = dstData.Scan0;

            // compute histograms for each channel
            int[] rHist = new int[256];
            int[] gHist = new int[256];
            int[] bHist = new int[256];

            unsafe
            {
                byte* srcPtr = (byte*)scan0Src;

                for (int y = 0; y < height; y++)
                {
                    byte* row = srcPtr + y * strideSrc;
                    for (int x = 0; x < width; x++)
                    {
                        bHist[row[x * 3 + 0]]++;
                        gHist[row[x * 3 + 1]]++;
                        rHist[row[x * 3 + 2]]++;
                    }
                }
            }

            // luts for eah channels
            byte[] rLUT = ComputeLUT(rHist, width * height);
            byte[] gLUT = ComputeLUT(gHist, width * height);
            byte[] bLUT = ComputeLUT(bHist, width * height);

            // apply
            unsafe
            {
                byte* srcPtr = (byte*)scan0Src;
                byte* dstPtr = (byte*)scan0Dst;

                Parallel.For(0, height, y =>
                {
                    byte* rowSrc = srcPtr + y * strideSrc;
                    byte* rowDst = dstPtr + y * strideDst;

                    for (int x = 0; x < width; x++)
                    {
                        rowDst[x * 3 + 0] = bLUT[rowSrc[x * 3 + 0]]; // B
                        rowDst[x * 3 + 1] = gLUT[rowSrc[x * 3 + 1]]; // G
                        rowDst[x * 3 + 2] = rLUT[rowSrc[x * 3 + 2]]; // R
                    }
                });
            }

            b.UnlockBits(srcData);
            result.UnlockBits(dstData);

            return result;
        }

        // helper method to compute LUT from histogram
        private byte[] ComputeLUT(int[] hist, int totalPixels)
        {
            int[] cumulativeDisFun = new int[256];
            cumulativeDisFun[0] = hist[0];
            for (int i = 1; i < 256; i++)
                cumulativeDisFun[i] = cumulativeDisFun[i - 1] + hist[i];

            byte[] lut = new byte[256];
            float scale = 255f / (totalPixels - cumulativeDisFun[0]);
            for (int i = 0; i < 256; i++)
            {
                lut[i] = (byte)Math.Round((cumulativeDisFun[i] - cumulativeDisFun[0]) * scale);
                if (lut[i] < 0) lut[i] = 0;
                if (lut[i] > 255) lut[i] = 255;
            }

            return lut;
        }

    }

}
