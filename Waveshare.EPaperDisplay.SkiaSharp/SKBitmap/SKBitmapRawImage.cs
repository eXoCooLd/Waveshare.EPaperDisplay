#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2022 Andre Wehrli

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
using Waveshare.Interfaces.Internal;

#endregion Usings

namespace Waveshare.Image
{
    /// <summary>
    /// Wrapper for a SKBitmap into a RAW Image for the E-Paper Display
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class SKBitmapRawImage : IRawImage
    {

        //########################################################################################

        #region Properties

        /// <summary>
        /// IntPointer to the Byte Array of the Image
        /// </summary>
        private IntPtr m_ScanLine;

        /// <summary>
        /// The SKBitmap used for the ScanLine
        /// </summary>
        private SkiaSharp.SKBitmap? Bitmap { get; set; }

        /// <summary>
        /// Width of the Image or Device Width
        /// </summary>
        public int Width => Bitmap != null ? Bitmap.Width : 0;

        /// <summary>
        /// Height of the Image or Device Height
        /// </summary>
        public int Height => Bitmap != null ? Bitmap.Height : 0;

        /// <summary>
        /// Used Bytes per Pixel
        /// </summary>
        public int BytesPerPixel => Bitmap != null ? Bitmap.BytesPerPixel : 0;

        /// <summary>
        /// Length of a ScanLine in Bytes
        /// </summary>
        public int Stride => Bitmap != null ? (Bitmap.Width * Bitmap.BytesPerPixel) : 0;

        /// <summary>
        /// IntPointer to the Byte Array of the Image
        /// </summary>
        public IntPtr ScanLine
        {
            get
            {
                if (m_ScanLine == IntPtr.Zero && Bitmap != null)
                {
                    m_ScanLine = Bitmap.GetPixels();
                }

                return m_ScanLine;
            }
        }

        #endregion Properties

        //########################################################################################

        #region Constructor / Dispose / Finalizer

        /// <summary>
        /// Constructor with the SKBitmap
        /// </summary>
        /// <param name="bitmap"></param>
        public SKBitmapRawImage(SkiaSharp.SKBitmap bitmap)
        {
            Bitmap = bitmap;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Bitmap?.Dispose();
            Bitmap = null;
        }

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

    }
}
