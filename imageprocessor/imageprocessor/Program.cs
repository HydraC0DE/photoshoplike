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


            

            string imagePath = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\darkscarycat_raw.png";
            Bitmap originalBitmap = new Bitmap(imagePath);


            // Histogram test using external save
            rawDarkScaryCat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\darkscarycat_raw.png");

            // Convert to grayscale first
            Grayscale gs = new Grayscale();
            Bitmap grayBitmapHistogram = gs.Apply8BitImage(new Bitmap(rawDarkScaryCat));

            // Create histogram calculator
            HistogramCalc histCalc = new HistogramCalc();

            // Generate histogram data
            int[] histogram = histCalc.CalculateHistogram8Bit(grayBitmapHistogram);
            int[] cumulativeHistogram = histCalc.CalculateCumulativeHistogram8bit(histogram);
            double[] normalizedHistogram = histCalc.CalculateNormalisedHistogram8Bit(grayBitmapHistogram);

            // Create histogram graph as bitmap
            Bitmap histogramGraph = histCalc.HistogramGraph(grayBitmapHistogram); // <- you’ll need to modify this to *return Bitmap* instead of saving internally

            // Save the graph manually like the other filters
            string outputPathHistogramGraph = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_scarycat_histogram_base.bmp";
            histogramGraph.Save(outputPathHistogramGraph);

            Console.WriteLine("Histogram graph saved at: " + outputPathHistogramGraph);


            // --- RGB histogram ---
            var (rHist, gHist, bHist) = histCalc.CalculateHistogram(originalBitmap);
            var (rCumulative, gCumulative, bCumulative) = histCalc.CalculateCumulativeHistogramRGB(rHist, gHist, bHist);
            var (rNorm, gNorm, bNorm) = histCalc.CalculateNormalizedHistogram(originalBitmap);

            Console.WriteLine("\nRGB histogram:");
            Console.WriteLine($"Red level 100 count: {rHist[100]}, normalized: {rNorm[100]:F4}");
            Console.WriteLine($"Green level 100 count: {gHist[100]}, normalized: {gNorm[100]:F4}");
            Console.WriteLine($"Blue level 100 count: {bHist[100]}, normalized: {bNorm[100]:F4}");

            Console.WriteLine("\nHistogram test finished.");


            //Laplace
            rawUglycat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\uglycat_raw.png");
            Laplace laplace = new Laplace();
            Bitmap laplacedUglyCat = laplace.ApplyLaplace4(rawUglycat);
            string laplacePath = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_uglycat_laplace.bmp";
            laplacedUglyCat.Save(laplacePath);

            //Sobel
            rawUglycat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\uglycat_raw.png");
            Sobel sobel = new Sobel();
            Bitmap sobelCat = sobel.ApplySobel(rawUglycat);
            string outputPathSobel = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_scarycat_sobel.bmp";
            sobelCat.Save(outputPathSobel);

            ;
        }
    }
}
