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

            rawUglycat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\uglycat_raw.png");
            Negation negation = new Negation();
            Bitmap grayImageHighBitDepth = negation.Negate(rawUglycat);
            string outputPath2 = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_uglycat_negate.bmp";
            grayImageHighBitDepth.Save(outputPath2);

            rawUglycat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\uglycat_raw.png");

            // --- Darker gamma ---
            GammaTrans gammaDarker = new GammaTrans();
            Bitmap darker = gammaDarker.ApplyGamma(new Bitmap(rawUglycat), 0.5, 0.5, 0.5); // higher gamma = darker
            string outputPathDarker = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_uglycat_gamma_darker.bmp";
            darker.Save(outputPathDarker);

            rawUglycat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\uglycat_raw.png");

            // --- Lighter gamma ---
            GammaTrans gammaLighter = new GammaTrans();
            Bitmap lighter = gammaLighter.ApplyGamma(new Bitmap(rawUglycat), 3, 3, 3); // lower gamma = lighter
            string outputPathLighter = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_uglycat_gamma_lighter.bmp";
            lighter.Save(outputPathLighter);

            rawUglycat = new Bitmap(@"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\uglycat_raw.png");

            // --- Only blue channel adjusted ---
            GammaTrans gammaBlue = new GammaTrans();
            Bitmap blueBoost = gammaBlue.ApplyGamma(new Bitmap(rawUglycat), 1, 1, 2); // make blue darker
            string outputPathBlue = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\output_uglycat_gamma_blue.bmp";
            blueBoost.Save(outputPathBlue);
            ;
        }
    }
}
