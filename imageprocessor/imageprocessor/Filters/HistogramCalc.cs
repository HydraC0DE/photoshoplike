using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageprocessor.Filters
{
    public class HistogramCalc //Only first 2 useful and optimized, graph also unoptimzed
    {
        public int[] CalculateHistogram8Bit(Bitmap grayBitmap) // 24 ms
        {
            int width = grayBitmap.Width;
            int height = grayBitmap.Height;

            BitmapData data = grayBitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format8bppIndexed);

            int stride = data.Stride;
            int[] globalHist = new int[256];

            unsafe
            {
                byte* basePtr = (byte*)data.Scan0;

                Parallel.ForEach(Partitioner.Create(0, height), range =>
                {
                    int[] localHist = new int[256];

                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        byte* row = basePtr + y * stride;
                        int x = 0;

                        // process 4 pixels at once
                        for (; x <= width - 4; x += 4)
                        {
                            uint block = *(uint*)(row + x);

                            localHist[(byte)(block)]++;
                            localHist[(byte)(block >> 8)]++;
                            localHist[(byte)(block >> 16)]++;
                            localHist[(byte)(block >> 24)]++;
                        }

                        // handle leftover pixels
                        for (; x < width; x++)
                            localHist[row[x]]++;
                    }

                    // merge into global
                    lock (globalHist)
                    {
                        for (int i = 0; i < 256; i++)
                            globalHist[i] += localHist[i];
                    }
                });
            }

            grayBitmap.UnlockBits(data);
            return globalHist;
        } //24ms

        public (int[] redHist, int[] greenHist, int[] blueHist) CalculateHistogramRGB(Bitmap bitmapOriginal) //100 ms
        {
            int width = bitmapOriginal.Width;
            int height = bitmapOriginal.Height;

            BitmapData data = bitmapOriginal.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            int stride = data.Stride;
            int[] red = new int[256];
            int[] green = new int[256];
            int[] blue = new int[256];

            unsafe
            {
                byte* basePtr = (byte*)data.Scan0;

                Parallel.ForEach(Partitioner.Create(0, height), range =>
                {
                    int[] rLocal = new int[256];
                    int[] gLocal = new int[256];
                    int[] bLocal = new int[256];

                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        byte* row = basePtr + y * stride;
                        int x = 0;

                        int maxUnroll = width - 3;

                        // unroll 4 pixels per iteration
                        for (; x < maxUnroll; x += 4)
                        {
                            // pixel 0
                            {
                                bLocal[row[0]]++;
                                gLocal[row[1]]++;
                                rLocal[row[2]]++;
                                row += 3;
                            }
                            // pixel 1
                            {
                                bLocal[row[0]]++;
                                gLocal[row[1]]++;
                                rLocal[row[2]]++;
                                row += 3;
                            }
                            // pixel 2
                            {
                                bLocal[row[0]]++;
                                gLocal[row[1]]++;
                                rLocal[row[2]]++;
                                row += 3;
                            }
                            // pixel 3
                            {
                                bLocal[row[0]]++;
                                gLocal[row[1]]++;
                                rLocal[row[2]]++;
                                row += 3;
                            }
                        }

                        // remaining pixels
                        for (; x < width; x++)
                        {
                            bLocal[row[0]]++;
                            gLocal[row[1]]++;
                            rLocal[row[2]]++;
                            row += 3;
                        }
                    }

                    lock (red)
                    {
                        for (int i = 0; i < 256; i++)
                        {
                            red[i] += rLocal[i];
                            green[i] += gLocal[i];
                            blue[i] += bLocal[i];
                        }
                    }
                });
            }

            bitmapOriginal.UnlockBits(data);
            return (red, green, blue);
        } //100 ms

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
            var (redHist, greenHist, blueHist) = CalculateHistogramRGB(bitmapOriginal);

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

        public Bitmap HistogramGraph(Bitmap bitmapOriginal)
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

            return histImage;
        }

    }
}
