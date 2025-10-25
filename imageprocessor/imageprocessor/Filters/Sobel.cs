using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageprocessor.Filters
{
    internal class Sobel
    {


        public Bitmap ApplySobel(Bitmap bitmapOriginal) // 350 ms
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData srcData = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            Bitmap resultBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData dstData = resultBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);

            int stride = srcData.Stride;

            unsafe
            {
                byte* srcBase = (byte*)srcData.Scan0;
                byte* dstBase = (byte*)dstData.Scan0;

                Parallel.For(1, height - 1, y =>
                {
                    //pointers to relevant rows
                    byte* prevRow = srcBase + (y - 1) * stride;
                    byte* curRow = srcBase + y * stride;
                    byte* nextRow = srcBase + (y + 1) * stride;
                    byte* dstRow = dstBase + y * stride;

                    for (int x = 1; x < width - 1; x++)//skip first and last column
                    {
                        int baseIdx = x * 3; //current
                        int leftIdx = baseIdx - 3; //left
                        int rightIdx = baseIdx + 3; // right
                         
                        // cache reused pixel positions for top, mid, bottom
                        byte* topLeft = prevRow + leftIdx;
                        byte* topCenter = prevRow + baseIdx;
                        byte* topRight = prevRow + rightIdx;

                        byte* midLeft = curRow + leftIdx;
                        byte* midRight = curRow + rightIdx;

                        byte* botLeft = nextRow + leftIdx;
                        byte* botCenter = nextRow + baseIdx;
                        byte* botRight = nextRow + rightIdx;

                        //-1 0 1
                        //-2 0 2
                        //-1 0 1
                        int gxB = (-topLeft[0] - 2 * midLeft[0] - botLeft[0]) + (topRight[0] + 2 * midRight[0] + botRight[0]);
                        int gxG = (-topLeft[1] - 2 * midLeft[1] - botLeft[1]) + (topRight[1] + 2 * midRight[1] + botRight[1]);
                        int gxR = (-topLeft[2] - 2 * midLeft[2] - botLeft[2]) + (topRight[2] + 2 * midRight[2] + botRight[2]);

                        //-1 -2 -1
                        //0 0 0
                        //1 2 1
                        int gyB = (-topLeft[0] - 2 * topCenter[0] - topRight[0]) + (botLeft[0] + 2 * botCenter[0] + botRight[0]);
                        int gyG = (-topLeft[1] - 2 * topCenter[1] - topRight[1]) + (botLeft[1] + 2 * botCenter[1] + botRight[1]);
                        int gyR = (-topLeft[2] - 2 * topCenter[2] - topRight[2]) + (botLeft[2] + 2 * botCenter[2] + botRight[2]);

                        int valB = Math.Min(255, Math.Abs(gxB) + Math.Abs(gyB));
                        int valG = Math.Min(255, Math.Abs(gxG) + Math.Abs(gyG));
                        int valR = Math.Min(255, Math.Abs(gxR) + Math.Abs(gyR));

                        byte* dstPixel = dstRow + baseIdx;
                        dstPixel[0] = (byte)valB;
                        dstPixel[1] = (byte)valG;
                        dstPixel[2] = (byte)valR;
                    }

                    //zero out left and right borders for consistency, but once again irrelevant
                    //dstRow[0] = dstRow[1] = dstRow[2] = 0;
                    //int rb = (width - 1) * 3;
                    //dstRow[rb + 0] = dstRow[rb + 1] = dstRow[rb + 2] = 0;
                });
            }

            bitmapOriginal.UnlockBits(srcData);
            resultBitmap.UnlockBits(dstData);

            return resultBitmap;
        }


    }
}
