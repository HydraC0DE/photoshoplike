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
        public Bitmap HistogramEqualize(Bitmap b)
        {
            int width = b.Width;
            int height = b.Height;

            // Create a new bitmap for the result
            Bitmap result = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            // Lock original bitmap
            BitmapData srcData = b.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            // Lock result bitmap
            BitmapData dstData = result.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int strideSrc = srcData.Stride;
            int strideDst = dstData.Stride;

            IntPtr scan0Src = srcData.Scan0;
            IntPtr scan0Dst = dstData.Scan0;

            // Compute histograms for each channel
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

            // Compute CDF and LUT for each channel
            byte[] rLUT = ComputeLUT(rHist, width * height);
            byte[] gLUT = ComputeLUT(gHist, width * height);
            byte[] bLUT = ComputeLUT(bHist, width * height);

            // Apply LUTs
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

        // Helper method to compute LUT from histogram
        private byte[] ComputeLUT(int[] hist, int totalPixels)
        {
            int[] cdf = new int[256];
            cdf[0] = hist[0];
            for (int i = 1; i < 256; i++)
                cdf[i] = cdf[i - 1] + hist[i];

            byte[] lut = new byte[256];
            float scale = 255f / (totalPixels - cdf[0]);
            for (int i = 0; i < 256; i++)
            {
                lut[i] = (byte)Math.Round((cdf[i] - cdf[0]) * scale);
                if (lut[i] < 0) lut[i] = 0;
                if (lut[i] > 255) lut[i] = 255;
            }

            return lut;
        }

    }

}
