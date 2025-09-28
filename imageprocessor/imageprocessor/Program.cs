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

            ;
        }
    }
}
