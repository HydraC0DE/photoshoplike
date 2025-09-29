using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageprocessor.Filters
{
    public class HistogramCalc
    {
        public int[] CalculateHistogram8Bit(Bitmap grayBitmap)
        {
            int width = grayBitmap.Width;
            int height = grayBitmap.Height;

            BitmapData bitmapData = grayBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format8bppIndexed);

            int stride = bitmapData.Stride;
            IntPtr Scan0 = bitmapData.Scan0;

            int[] histogram = new int[256];

            unsafe
            {
                byte* basePointer = (byte*)Scan0;

                Parallel.For(0, height, y =>
                {
                    byte* row = basePointer + (y * stride);

                    for (int x = 0; x < width; x++)
                    {
                        byte gray = row[x]; // directly read the grayscale value
                        System.Threading.Interlocked.Increment(ref histogram[gray]);
                    }
                });
            }

            grayBitmap.UnlockBits(bitmapData);

            return histogram;
        }
        public (int[] redHist, int[] greenHist, int[] blueHist) CalculateHistogram(Bitmap bitmapOriginal)
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData bitmapData = bitmapOriginal.LockBits(
        new Rectangle(0, 0, width, height),
        ImageLockMode.ReadWrite,
        PixelFormat.Format24bppRgb);

            int stride = bitmapData.Stride;
            IntPtr Scan0 = bitmapData.Scan0;

            int[] redHist = new int[256];
            int[] greenHist = new int[256];
            int[] blueHist = new int[256];

            unsafe
            {
                byte* basePointer = (byte*)Scan0;

                Parallel.For(0, height, y =>
                {
                    byte* row = basePointer + (y * stride);
                    for (int x = 0; x < width; x++)
                    {
                        byte b = row[x * 3 + 0];
                        byte g = row[x * 3 + 1];
                        byte r = row[x * 3 + 2];


                        System.Threading.Interlocked.Increment(ref redHist[r]); //avoid parallel issues
                        System.Threading.Interlocked.Increment(ref greenHist[g]);
                        System.Threading.Interlocked.Increment(ref blueHist[b]);
                    }
                });
            }


            bitmapOriginal.UnlockBits(bitmapData);

            return (redHist, greenHist, blueHist);
        }

        public int[] CalculateCumulativeHistogram8bit(int[] histogram)
        {
            int[] cumulativeHistogram = new int[histogram.Length];
            int cumulative = 0;
            for (int i = 0; i < histogram.Length; i++)
            {
                cumulative += histogram[i];
                cumulativeHistogram[i] = cumulative;
            }

            return cumulativeHistogram;
        }

        public (int[] redCumulative, int[] greenCumulative, int[] blueCumulative) CalculateCumulativeHistogramRGB(int[] redHist, int[] greenHist, int[] blueHist)
        {
            int[] redCumulativeArr = new int[redHist.Length];
            int[] greenCumulativeArr = new int[greenHist.Length];
            int[] blueCumulativeArr = new int[blueHist.Length];

            int cumulativeR = 0;
            int cumulativeG = 0;
            int cumulativeB = 0;

            for (int i = 0; i < redHist.Length; i++)
            {
                cumulativeR += redHist[i];
                cumulativeG += greenHist[i];
                cumulativeB += blueHist[i];

                redCumulativeArr[i] = cumulativeR;
                greenCumulativeArr[i] = cumulativeG;
                blueCumulativeArr[i] = cumulativeB;
            }

            return (redCumulativeArr, greenCumulativeArr, blueCumulativeArr);
        }

        public double[] CalculateNormalisedHistogram8Bit(Bitmap bitmapOriginal)
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;
            int totalPixels = width * height;

            double[] histNorm = new double[256];

            int[] histogram = CalculateHistogram8Bit(bitmapOriginal);

            for (int i = 0; i < 256; i++)
            {
                histNorm[i] = histogram[i] / (double)totalPixels;
            }

            return histNorm;
        }

        public (double[] redHistNorm, double[] greenHistNorm, double[] blueHistNorm) CalculateNormalizedHistogram(Bitmap bitmapOriginal)
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;
            int totalPixels = width * height;

            // first, get the regular histogram
            var (redHist, greenHist, blueHist) = CalculateHistogram(bitmapOriginal);

            double[] redHistNorm = new double[256];
            double[] greenHistNorm = new double[256];
            double[] blueHistNorm = new double[256];

            for (int i = 0; i < 256; i++)
            {
                redHistNorm[i] = redHist[i] / (double)totalPixels;
                greenHistNorm[i] = greenHist[i] / (double)totalPixels;
                blueHistNorm[i] = blueHist[i] / (double)totalPixels;
            }

            return (redHistNorm, greenHistNorm, blueHistNorm); //redHistNorm[100] = 0.05 means that 5% of al pixels have red = 100
        }

        public void HistogramGraph(Bitmap bitmapOriginal)
        {
            int[] histogram = CalculateHistogram8Bit(bitmapOriginal);
            int width = 256; // one pixel per intensity
            int height = 100; // graph height
            Bitmap histImage = new Bitmap(width, height);
            int max = histogram.Max();

            using (Graphics g = Graphics.FromImage(histImage))
            {
                g.Clear(Color.White);
                for (int i = 0; i < 256; i++)
                {
                    int barHeight = (int)(histogram[i] / (float)max * height);
                    g.FillRectangle(Brushes.Black, i, height - barHeight, 1, barHeight);
                }
            }
            histImage.Save("histogram.png");

        }
    }
}
