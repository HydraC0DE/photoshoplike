using imageprocessor.Filters;
using System.Drawing;

namespace imageprocessor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Bitmap rawUglycat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\uglycat_raw.png");

            Grayscale grayscale = new Grayscale();
            Bitmap grayImage = grayscale.FastApply(rawUglycat);

            string outputPath = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_uglycat_gray.bmp";
            grayImage.Save(outputPath);
            //negate
            rawUglycat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\uglycat_raw.png");
            Negation negation = new Negation();
            Bitmap grayImageHighBitDepth = negation.Negate(rawUglycat);
            string outputPath2 = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_uglycat_negate.bmp";
            grayImageHighBitDepth.Save(outputPath2);



            // darker gamma
            rawUglycat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\uglycat_raw.png");
            GammaTrans gammaDarker = new GammaTrans();
            Bitmap darker = gammaDarker.ApplyGamma(new Bitmap(rawUglycat), 0.5, 0.5, 0.5); // lower gamma = darker
            string outputPathDarker = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_uglycat_gamma_darker.bmp";
            darker.Save(outputPathDarker);



            // ligher gamma
            rawUglycat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\uglycat_raw.png");
            GammaTrans gammaLighter = new GammaTrans();
            Bitmap lighter = gammaLighter.ApplyGamma(new Bitmap(rawUglycat), 3, 3, 3); // higher gamma = lighter
            string outputPathLighter = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_uglycat_gamma_lighter.bmp";
            lighter.Save(outputPathLighter);



            // blue gamma
            rawUglycat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\uglycat_raw.png");
            GammaTrans gammaBlue = new GammaTrans();
            Bitmap blueBoost = gammaBlue.ApplyGamma(new Bitmap(rawUglycat), 1, 1, 2); // make blue darker
            string outputPathBlue = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_uglycat_gamma_blue.bmp";
            blueBoost.Save(outputPathBlue);

            //logarithmicHighValue
            Bitmap rawDarkScaryCat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\darkscarycat_raw.png");
            LogarithmicTrans logarithmicTooHigh = new LogarithmicTrans();
            Bitmap logtranshigh = logarithmicTooHigh.ApplyLogartihmic(new Bitmap(rawDarkScaryCat), 70);
            string outputPathLogaritmicHigh = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_scarycat_log_high.bmp";
            logtranshigh.Save(outputPathLogaritmicHigh);

            //logarithmicBaseValue
            rawDarkScaryCat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\darkscarycat_raw.png");
            LogarithmicTrans logarithmicBaseVal = new LogarithmicTrans();
            Bitmap logtransbase = logarithmicBaseVal.ApplyLogartihmic(new Bitmap(rawDarkScaryCat));
            string outputPathLogaritmicBase = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_scarycat_log_base.bmp";
            logtransbase.Save(outputPathLogaritmicBase);


            Console.WriteLine("Histogram test starting...");

            string imagePath = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\darkscarycat_raw.png";
            Bitmap originalBitmap = new Bitmap(imagePath);

            HistogramCalc histCalc = new HistogramCalc();
            Grayscale gs = new Grayscale();

            // --- 8-bit grayscale histogram ---
            Bitmap grayBitmap = gs.Apply8BitImage(originalBitmap); // convert to 8-bit grayscale
            int[] grayHist = histCalc.CalculateHistogram8Bit(grayBitmap);
            int[] grayCumulative = histCalc.CalculateCumulativeHistogram8bit(grayHist);
            double[] grayNorm = histCalc.CalculateNormalisedHistogram8Bit(grayBitmap);

            Console.WriteLine("8-bit grayscale histogram:");
            Console.WriteLine($"Gray level 100 count: {grayHist[100]}");
            Console.WriteLine($"Cumulative at 100: {grayCumulative[100]}");
            Console.WriteLine($"Normalized at 100: {grayNorm[100]:F4}");

            // Generate graph for grayscale
            histCalc.HistogramGraph(grayBitmap);
            Console.WriteLine("Grayscale histogram graph saved as histogram.png");

            // --- RGB histogram ---
            var (rHist, gHist, bHist) = histCalc.CalculateHistogram(originalBitmap);
            var (rCumulative, gCumulative, bCumulative) = histCalc.CalculateCumulativeHistogramRGB(rHist, gHist, bHist);
            var (rNorm, gNorm, bNorm) = histCalc.CalculateNormalizedHistogram(originalBitmap);

            Console.WriteLine("\nRGB histogram:");
            Console.WriteLine($"Red level 100 count: {rHist[100]}, normalized: {rNorm[100]:F4}");
            Console.WriteLine($"Green level 100 count: {gHist[100]}, normalized: {gNorm[100]:F4}");
            Console.WriteLine($"Blue level 100 count: {bHist[100]}, normalized: {bNorm[100]:F4}");

            Console.WriteLine("\nHistogram test finished.");

            ;
        }
    }
}
