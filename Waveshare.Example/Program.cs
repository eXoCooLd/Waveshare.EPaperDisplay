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
using Waveshare.Devices;
#endregion Usings

namespace Waveshare.Example
{
    /// <summary>
    /// Example for the Waveshare E-Paper Library
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application Main
        /// </summary>
        public static void Main()
        {
            const string fileName = "like_a_sir_640x385.bmp";
            using var bitmap = new Bitmap(Image.FromFile(fileName, true));

            using var ePaperDisplay = EPaperDisplay.Create(EPaperDisplayType.WaveShare7In5Bc);

            ePaperDisplay.Clear();
            ePaperDisplay.WaitUntilReady();

            Console.WriteLine($"Sending {fileName} to E-Paper Display...");
            ePaperDisplay.DisplayImage(bitmap);
            Console.WriteLine("Done");

            ePaperDisplay.Sleep();
        }
    }
}
