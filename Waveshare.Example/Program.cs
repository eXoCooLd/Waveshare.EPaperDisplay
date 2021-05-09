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
using System.Drawing;
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
            using var bitmap = LoadBitmap(args);
            if (bitmap == null)
            {
                return;
            }

            using var ePaperDisplay = EPaperDisplay.Create(EPaperDisplayType.WaveShare7In5b_V2);

            ePaperDisplay.Clear();
            ePaperDisplay.WaitUntilReady();

            Console.WriteLine("Sending Image to E-Paper Display...");
            ePaperDisplay.DisplayImage(bitmap, true);
            Console.WriteLine("Done");

            ePaperDisplay.Sleep();
        }

        #endregion Public Methods

        //########################################################################################

        #region Private Methods

        /// <summary>
        /// Load Bitmap from arguments or get the default bitmap
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Bitmap LoadBitmap(string[] args)
        {
            string bitmapFilePath;

            if (args == null || args.Length == 0)
            {
                const string fileName = "like_a_sir_800x480.bmp";
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

            return new Bitmap(bitmapFilePath);
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