using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace imageprocessor
{
    public class ImageHander
    {
        public static Bitmap Load(string path)
        {
            return new Bitmap(path);
        }

        public static void Save(Bitmap bitmap, string path)
        {
            bitmap.Save(path);
        }
    }
}
