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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using Waveshare.Interfaces.Internal;

#endregion Usings

namespace Waveshare.Image
{
    /// <summary>
    /// RAW Image Wrapper of a Bitmap
    /// </summary>
    internal sealed class BitmapRawImage : IRawImage
    {

        //########################################################################################

        #region Properties

        /// <summary>
        /// IntPointer to the Byte Array of the Image
        /// </summary>
        private IntPtr m_ScanLine;

        /// <summary>
        /// The Bitmap used for the ScanLine
        /// </summary>
        private System.Drawing.Bitmap? Bitmap { get; set; }

        /// <summary>
        /// The raw Bitmap Data
        /// </summary>
        private BitmapData? BitmapData { get; set; }

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
        public int BytesPerPixel { get; }

        /// <summary>
        /// Length of a ScanLine in Bytes
        /// </summary>
        public int Stride => Bitmap != null ? (Bitmap.Width * BytesPerPixel) : 0;

        /// <summary>
        /// IntPointer to the Byte Array of the Image
        /// </summary>
        public IntPtr ScanLine
        {
            get
            {
                if (m_ScanLine == IntPtr.Zero && BitmapData != null)
                {
                    m_ScanLine = BitmapData.Scan0;
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
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        public BitmapRawImage(System.Drawing.Bitmap bitmap, int maxWidth, int maxHeight)
        {
            Bitmap = bitmap;
            BitmapData = bitmap.LockBits(new Rectangle(0, 0, maxWidth, maxHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BytesPerPixel = BitmapData.Stride / bitmap.Width;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Bitmap?.UnlockBits(BitmapData);
                BitmapData = null;
                m_ScanLine = IntPtr.Zero;

                //Do not dispose the Bitmap, because we did not create it.
                //We do not know if it is used after the display refresh
                Bitmap = null;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~BitmapRawImage() => Dispose(false);

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

    }
}
