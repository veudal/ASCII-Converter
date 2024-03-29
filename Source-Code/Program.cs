﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageInConsole
{
    class Program
    {
        //Based on .NET Framework 
        //original { " ", ".", "-", ":", "*", "+", "=", "%", "@", "#", "#" }
        private static readonly string[] _asciiChars = { " ", ".", "-", ":", "*", "+", "=", "%", "@", "#", " " };
        private const int _asciiWidth = 150;
        static string path = "";
        static bool play = false;
        static bool hasEnded = false;
        static string[] asciiArray;

        static void Main()
        {
            SetConsoleProperties();
            while (true)
            {
                try
                {
                    Console.WriteLine("Write the path to the file or the URL to the direct image or Gif. Tip: you can also write 'time'.");
                    path = Console.ReadLine();
                    if (path.ToLower() != "time")
                    {
                        string upperCasePath = path.ToUpper();
                        if (upperCasePath.EndsWith(".GIF"))
                        {
                            ASCIIGif();
                        }
                        else
                        {
                            ASCIIImage();
                        }
                    }
                    else
                    {
                        ASCIIClock();
                    }
                }
                catch
                {
                    Console.WriteLine("File could not be converted, make sure it is an image or Gif file, if you entered a remote file (same goes for local file) and it is an Gif, then check if the URL ends with '.Gif'.");
                }
            }

        }

        private static void SetConsoleProperties()
        {
            Console.Title = "ASCII Converter by SagMeinenNamen";
            bool maxHeightSize = true;
            bool maxWidthSize = true;
            int height = 60;
            int Width = 200;
            while (maxHeightSize == true)
            {
                try
                {
                    height--;
                    Console.WindowHeight = height;
                    maxHeightSize = false;
                }
                catch
                {
                }
            }
            while (maxWidthSize == true)
            {
                try
                {
                    Width--;
                    Console.WindowWidth = Width;
                    maxWidthSize = false;
                }
                catch
                {
                }
            }
            Console.CursorVisible = true;
        }

        private static void ASCIIGif()
        {
            play = true;
            Thread thr = new Thread(GifAnimation);
            thr.Start();
            Console.ReadLine();
            play = false;
            while (hasEnded == false)
            {
                Task.Delay(100);
            }
            Console.Clear();
        }

        private static void GifAnimation()
        {
            try
            {
                hasEnded = false;
                PrepareGif(path); //fill array with all string images
                int length = asciiArray.Length;
                Console.Clear();
                while (play) //check if user presses enter
                {
                    for (int i = 0; i < length && play == true; i++)
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        if (i.ToString().EndsWith("0"))
                        {
                            Console.CursorVisible = false;
                        }
                        WriteImage(asciiArray, i);
                        WaitForNextFrame(stopwatch); //if frame 'renders' too fast, then there will be a delay before the next image.

                    }
                }
                hasEnded = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("File could not be converted, make sure it is an image or Gif file, if you entered a remote file (same goes for local file) and it is an Gif, then check if the URL ends with '.Gif'. Error: " + e.Message);
                hasEnded = true;
            }
        }

        private static void WriteImage(string[] asciiArray, int n)
        {
            //get the string of the current frame from the array
            string ascii = asciiArray[n];
            Console.Write(ascii);
            Console.CursorLeft = 0;
            Console.CursorTop = 0;
        }

        private static void WaitForNextFrame(Stopwatch stopwatch)
        {
            //calculates the time to wait before the next frames 'renders'
            int seconds = 55 - Convert.ToInt32(stopwatch.ElapsedMilliseconds);
            if (seconds > 1)
            {
                Thread.Sleep(seconds);
            }
        }

        private static void PrepareGif(string path)
        {
            Console.WriteLine("Loading...");
            Bitmap bitmap;
            if (IsLocalPath(path) == false)
            {
                bitmap = GetBitmapFromUrl(path);
                //download file if it is a remote path
            }
            else
            {
                bitmap = new Bitmap(path);
            }
            //Convert each frame of the Gif into a string array
            ConvertToAsciiArray(bitmap);
        }

        static void ConvertToAsciiArray(Bitmap originalImg)
        {
            int numberOfFrames = originalImg.GetFrameCount(FrameDimension.Time);
            asciiArray = new string[numberOfFrames];
            for (int i = 0; i < numberOfFrames; i++)
            {
                originalImg.SelectActiveFrame(FrameDimension.Time, i);
                asciiArray[i] = ConvertImageToAsciiArt(((Bitmap)originalImg.Clone()));
                i++;
                GC.Collect();
            }
        }

        private static void ASCIIClock()
        {
            play = true;
            Console.CursorVisible = false;
            Task.Run(Console.Clear);
            Thread thr = new Thread(LiveClock);
            thr.Start();
            Console.ReadLine();
            play = false;
            while (hasEnded == false)
            {
                Task.Delay(100);
            }
            Console.Clear();
        }

        private static void LiveClock()
        {
            hasEnded = false;
            string current = DateTime.Now.AddSeconds(-1).ToLongTimeString();
            while (play == true)
            {
                Console.CursorVisible = false;
                while (current == DateTime.Now.ToLongTimeString() && play == true)
                {
                    Thread.Sleep(200);
                }
                string ascii = ConvertTimeToAscii();
                Console.Write(ascii);
                Console.CursorLeft = 0;
                Console.CursorTop = 0;
                current = DateTime.Now.ToLongTimeString();
            }
            hasEnded = true;
        }

        private static string ConvertTimeToAscii()
        {
            Bitmap bmp = new Bitmap(900, 300);
            RectangleF rectf = new RectangleF(0, 0, bmp.Width, bmp.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawString(DateTime.Now.ToString("T"), new Font(FontFamily.GenericSerif, 160), Brushes.DarkOliveGreen, rectf);
            g.Flush();
            //bmp.Save(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\aPicture.bmp");
            return ConvertImageToAsciiArt(bmp); ;
        }

        private static void ASCIIImage()
        {
            Bitmap bitmap;
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
            int asciiHeight;
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

