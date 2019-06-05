using System;
using System.Drawing;

namespace ImageToRGBA
{
    class Program
    {
        static void Main(string[] args)
        {
            var image = new Bitmap("wave_engine_64.png");

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    var pixel = image.GetPixel(i, j);
                    Console.Write(
                        $"0x{pixel.R.ToString("x2")}, 0x{pixel.G.ToString("x2")}, " +
                        $"0x{pixel.B.ToString("x2")}, 0x{pixel.A.ToString("x2")}, ");
                }
            }

            Console.ReadKey();
        }
    }
}
