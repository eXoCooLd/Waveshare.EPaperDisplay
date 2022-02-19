#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2019 Andre Wehrli

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// --------------------------------------------------------------------------------------------------------------------
#endregion Copyright

#region Usings

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Waveshare.Devices;

#endregion Usings

namespace Waveshare.Example
{
    /// <summary>
    /// Example for the Waveshare E-Paper Library
    /// </summary>
    internal class Program
    {

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Application Main
        /// </summary>
        /// <param name="args">Commandline arguments</param>
        public static void Main(string[] args)
        {
            Console.Write("Initializing E-Paper Display...");
            var time = Stopwatch.StartNew();
            using var ePaperDisplay = EPaperDisplay.Create(EPaperDisplayType.WaveShare7In5Bc);
            time.Stop();
            Console.WriteLine($" [Done {time.ElapsedMilliseconds} ms]");

            using var bitmap = LoadBitmap(args, ePaperDisplay.Width, ePaperDisplay.Height);
            if (bitmap == null)
            {
                return;
            }

            Console.Write("Waiting for E-Paper Display...");
            time = Stopwatch.StartNew();
            ePaperDisplay.Clear();
            ePaperDisplay.WaitUntilReady();
            time.Stop();
            Console.WriteLine($" [Done {time.ElapsedMilliseconds} ms]");

            Console.Write("Sending Image to E-Paper Display...");
            time = Stopwatch.StartNew();
            ePaperDisplay.DisplayImage(bitmap, false);
            time.Stop();
            Console.WriteLine($" [Done {time.ElapsedMilliseconds} ms]");

            Console.WriteLine("Done");
        }

        #endregion Public Methods

        //########################################################################################

        #region Private Methods

        /// <summary>
        /// Load Bitmap from arguments or get the default bitmap
        /// </summary>
        /// <param name="args"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private static SkiaSharp.SKBitmap LoadBitmap(string[] args, int width, int height)
        {
            string bitmapFilePath;

            if (args == null || args.Length == 0)
            {
                var fileName = $"like_a_sir_{width}x{height}.bmp";
                bitmapFilePath = Path.Combine(ExecutingAssemblyPath, fileName);
            }
            else
            {
                bitmapFilePath = args.First();
            }

            if (!File.Exists(bitmapFilePath))
            {
                Console.WriteLine($"Can not find Bitmap file: '{bitmapFilePath}'!");
                return null;
            }

            return LoadSKBitmapFromFile(bitmapFilePath);
        }

        /// <summary>
        /// Load a SKBitmap from a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static SkiaSharp.SKBitmap LoadSKBitmapFromFile(string filePath)
        {
            SkiaSharp.SKBitmap bitmap;
            using (var stream = File.OpenRead(filePath))
            {
                bitmap = SkiaSharp.SKBitmap.Decode(stream);
            }

            return bitmap;
        }

        /// <summary>
        /// Return the path of the executing assembly
        /// </summary>
        private static string ExecutingAssemblyPath
        {
            get
            {
                var path = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(path);
            }
        }

        #endregion Private Methods

        //########################################################################################

    }
}