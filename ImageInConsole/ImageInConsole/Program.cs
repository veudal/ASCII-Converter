using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageToASCII
{
    class Program
    {
        //{ " ", ".", "-", ":", "*", "+", "=", "%", "@", "#", "#" }
        private static string[] _asciiChars = { " ", ".", "-", ":", "*", "+", "=", "%", "@", "#", "#" };
        private const int _asciiWidth = 150;
        static string path = "";

        static void Main(string[] args)
        {
            Console.Title = "ASCII Converter by SagMeinenNamen";
            Console.WindowWidth = 153;
            Console.WindowHeight = 65;
            while (true)
            {
                try
                {
                    
                    Console.CursorVisible = true;
                    Console.WriteLine("Write the path to the file or the URL to the direct image or gif (gif files need to end with '.gif' also as URL):");
                    path = Console.ReadLine();

                    if (path.EndsWith(".gif"))
                    {
                        ASCIIGif();
                    }
                    else
                    {
                        ASCIIImage();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }

        private static void ASCIIGif()
        {
            Thread thr = new Thread(RenderGif);
            thr.Start();
            Console.ReadLine();
            thr.Abort();
            Console.Clear();
        }

        private static void RenderGif()
        {
            Bitmap[] imgArray;
            string[] asciiArray;
            PrepareGif(path, out imgArray, out asciiArray);
            Console.Clear();
            while (true)
            {
                int n = 0;
                foreach (var I in imgArray)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    WriteImage(asciiArray, n);
                    n++;
                    WaitForNextFrame(stopwatch);

                }
            }
        }

        private static void WriteImage(string[] asciiArray, int n)
        {
            string ascii = asciiArray[n];
            Console.Write(ascii);
            Console.CursorLeft = 0;
            Console.CursorTop = 0;
        }

        private static void WaitForNextFrame(Stopwatch stopwatch)
        {
            int seconds = 80 - Convert.ToInt32(stopwatch.ElapsedMilliseconds);
            if (seconds > 1)
            {
                Thread.Sleep(seconds);
            }
        }

        private static void PrepareGif(string path, out Bitmap[] imgArray, out string[] asciiArray)
        {
            Console.CursorVisible = false;
            Console.WriteLine("Loading...");
            Bitmap bitmap;
            if (IsLocalPath(path) == true)
            {
                bitmap = new Bitmap(path);
            }
            else
            {
                bitmap = GetBitmapFromUrl(path);
            }
            imgArray = getFrames(bitmap);
            int i = 0;
            asciiArray = new string[imgArray.Length];
            foreach (var I in imgArray)
            {
                asciiArray[i] = ConvertImageToAsciiArt(I);
                i++;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);

        private static void Maximize()
        {
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
        }

        static Bitmap[] getFrames(Bitmap originalImg)
        {
            int numberOfFrames = originalImg.GetFrameCount(FrameDimension.Time);
            Bitmap[] frames = new Bitmap[numberOfFrames];

            for (int i = 0; i < numberOfFrames; i++)
            {
                originalImg.SelectActiveFrame(FrameDimension.Time, i);
                frames[i] = ((Bitmap)originalImg.Clone());
            }

            return frames;
        }

        private static void ASCIIImage()
        {
            Bitmap bitmap = null;
            if (IsLocalPath(path) == true)
            {
                bitmap = new Bitmap(path);
            }
            else
            {
                bitmap = GetBitmapFromUrl(path);
            }
            string ascii = ConvertImageToAsciiArt(bitmap);
            Console.Write(ascii);
            Console.ReadLine();
            Console.Clear();
        }

        private static bool IsLocalPath(string p)
        {
            if (p.StartsWith("http:\\") || p.StartsWith($"https://"))
            {
                return false;
            }

            return new Uri(p).IsFile;
        }

        private static Bitmap GetBitmapFromUrl(string remoteImageUrl)
        {
            WebRequest request = WebRequest.Create(remoteImageUrl);
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            Bitmap bitmap = new Bitmap(responseStream);
            return bitmap;
        }

        private static string ConvertImageToAsciiArt(Bitmap image)
        {
            image = GetReSizedImage(image, _asciiWidth);

            //Convert the resized image into ASCII
            string ascii = ConvertToAscii(image);
            return ascii;
        }

        private static Bitmap GetReSizedImage(Bitmap inputBitmap, int asciiWidth)
        {
            int asciiHeight = 0;
            //Calculate the new Height of the image from its width
            asciiHeight = (int)Math.Ceiling((double)inputBitmap.Height * asciiWidth / inputBitmap.Width);

            //Create a new Bitmap and define its resolution
            Bitmap result = new Bitmap(asciiWidth, asciiHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)result);
            //The interpolation mode produces high quality images 
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(inputBitmap, 0, 0, asciiWidth, asciiHeight);
            g.Dispose();
            return result;
        }

        private static string ConvertToAscii(Bitmap image)
        {
            Boolean toggle = false;
            StringBuilder sb = new StringBuilder();

            for (int h = 0; h < image.Height; h++)
            {
                for (int w = 0; w < image.Width; w++)
                {
                    System.Drawing.Color pixelColor = image.GetPixel(w, h);
                    //Average out the RGB components to find the Gray Color
                    int red = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int green = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int blue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    System.Drawing.Color grayColor = System.Drawing.Color.FromArgb(red, green, blue);

                    //Use the toggle flag to minimize height-wise stretch
                    if (!toggle)
                    {
                        int index = (grayColor.R * 10) / 255;
                        sb.Append(_asciiChars[index]);
                    }
                }

                if (!toggle)
                {
                    sb.Append(Environment.NewLine);
                    toggle = true;
                }
                else
                {
                    toggle = false;
                }
            }

            return sb.ToString();
        }
    }
}
