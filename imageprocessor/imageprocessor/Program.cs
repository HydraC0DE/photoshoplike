using System;
using System.Diagnostics;
using System.Drawing;
using imageprocessor.Filters;

namespace imageprocessor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Simple Filter Tester ===");
            Console.Write("Enter full image path: ");
            string inputPath = @"D:\DEVELOPMENT\DevRepos\photoshoplike\pictures\8k_test.jpg"; //@""


            Console.WriteLine("\nChoose a filter to apply:");
            Console.WriteLine("1 - Grayscale (FastApply)");
            Console.WriteLine("2 - Grayscale (8-bit)");
            Console.WriteLine("3 - Negation");
            Console.WriteLine("4 - Gamma (lighter)");
            Console.WriteLine("5 - Gamma (darker)");
            Console.WriteLine("6 - Laplace");
            Console.WriteLine("7 - Sobel");
            Console.WriteLine("8 - Box Filter");
            Console.WriteLine("9 - Gaussian Filter");
            Console.WriteLine("10 - Feature Point Detection");
            Console.WriteLine("11 - Histogram Equalization (8-bit grayscale)");
            Console.Write("\nEnter your choice (1–11): ");

            string choice = Console.ReadLine() ?? "";
            if (!int.TryParse(choice, out int selected) || selected < 1 || selected > 11)
            {
                Console.WriteLine("Invalid choice.");
                return;
            }

            Bitmap input = new Bitmap(inputPath);
            Bitmap output = null!;
            string outputPath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(inputPath)!,
                $"output_{System.IO.Path.GetFileNameWithoutExtension(inputPath)}_filter{selected}.bmp"
            );

            Stopwatch sw = new Stopwatch();
            

            switch (selected)
            {
                case 1:
                    sw.Start();
                    output = new Grayscale().FastApply(input);
                    break;

                case 2:
                    sw.Start();
                    output = new Grayscale().Apply8BitImage(input);
                    break;

                case 3:
                    sw.Start();
                    output = new Negation().Negate(input);
                    break;

                case 4:
                    sw.Start();
                    output = new GammaTrans().ApplyGamma(input, 3, 3, 3);
                    break;

                case 5:
                    sw.Start();
                    output = new GammaTrans().ApplyGamma(input, 0.5, 0.5, 0.5);
                    break;

                case 6:
                    sw.Start();
                    output = new Laplace().ApplyLaplace4_CacheFriendly(input);
                    break;

                case 7:
                    sw.Start();
                    output = new Sobel().ApplySobel(input);
                    break;

                case 8:
                    sw.Start();
                    output = new BoxFilter().ApplyBoxFilter(input);
                    break;

                case 9:
                    sw.Start();
                    output = new GaussianFilter().ApplyGaussianFilterFast(input);
                    break;

                case 10:
                    sw.Start();
                    output = new FeaturePoint().DetectCorners(input);
                    break;

                case 11:
                    sw.Start();
                    output = new HistogramEqualization().HistogramEqualize(input);
                    break;
            }

            sw.Stop();

            output.Save(outputPath);
            Console.WriteLine($"\nFilter {selected} complete!");
            Console.WriteLine($"Processing time: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine($"Saved to: {outputPath}");

            input.Dispose();
            output.Dispose();
        }
    }
}
