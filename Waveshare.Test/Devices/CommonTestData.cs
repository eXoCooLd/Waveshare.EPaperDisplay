﻿#region Copyright
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

using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;

#endregion Usings

namespace Waveshare.Test.Devices
{
    /// <summary>
    /// Helper Class to generate TestData for the UnitTests
    /// </summary>
    internal class CommonTestData
    {

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Create a Sample Bitmap
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static SKBitmap CreateSampleBitmap(int width, int height)
        {
            var image = new SKBitmap(new SkiaSharp.SKImageInfo(width, height, SKColorType.Bgra8888));

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var color = SKColors.White;

                    if (x % 2 == 0)
                    {
                        color = SKColors.Black;
                    }

                    if (x % 3 == 0)
                    {
                        color = SKColors.Red;
                    }

                    if (x % 4 == 0)
                    {
                        color = SKColors.Gray;
                    }

                    if (x % 5 == 0)
                    {
                        color = new SKColor(255, 50, 0, 0);
                    }

                    image.SetPixel(x, y, color);
                }
            }

            return image;
        }

        #endregion Public Methods

        //########################################################################################

    }
}