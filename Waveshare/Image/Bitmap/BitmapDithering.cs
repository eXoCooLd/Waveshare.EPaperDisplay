#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2021 Andre Wehrli

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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;

#endregion Usings

namespace Waveshare.Image.Bitmap
{
    /// <summary>
    /// Wrapper for Image Dithering with list of supported colors
    /// </summary>
    internal class BitmapDithering
    {

        //########################################################################################

        #region Properies

        /// <summary>
        /// Array with all supported colors of the device
        /// </summary>
        private Color[] SupportedColors { get; }

        /// <summary>
        /// Used Quantizer for color reduction in the dithering
        /// </summary>
        private IQuantizer Quantizer { get; }

        /// <summary>
        /// Used dithering algorithm
        /// </summary>
        private IDitherer Ditherer { get; }

        #endregion Properies

        //########################################################################################

        #region Constructor / Dispose / Finalizer

        /// <summary>
        /// Constructor with all supported Colors as byte arrays
        /// </summary>
        /// <param name="supportedByteColors"></param>
        public BitmapDithering(IList<byte[]> supportedByteColors)
        {
            SupportedColors = supportedByteColors.Select(c => Color.FromArgb(c[0], c[1], c[2])).ToArray();
            Quantizer = PredefinedColorsQuantizer.FromCustomPalette(SupportedColors);
            Ditherer = ErrorDiffusionDitherer.FloydSteinberg;
        }

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Image dithering method
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public System.Drawing.Bitmap Dither(System.Drawing.Bitmap input)
        {
            return input.ConvertPixelFormat(Quantizer.PixelFormatHint, Quantizer, Ditherer);
        }

        #endregion Public Methods

        //########################################################################################

    }
}