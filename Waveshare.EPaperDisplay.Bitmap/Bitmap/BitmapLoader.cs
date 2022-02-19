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
using Waveshare.Image;
using Waveshare.Interfaces;
using Waveshare.Interfaces.Internal;

#endregion Usings

namespace Waveshare.Image
{
    /// <summary>
    /// System.Drawing.Bitmap Image Loader
    /// </summary>
    internal class BitmapLoader : EPaperImageBase<System.Drawing.Bitmap>, IEPaperDisplayBitmap
    {

        //########################################################################################

        #region Constructor / Dispose / Finalizer

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ePaperDisplay"></param>
        public BitmapLoader(IEPaperDisplayInternal ePaperDisplay) : base(ePaperDisplay)
        {
        }

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

        #region Protected Methods

        /// <summary>
        /// Load the System.Drawing.Bitmap into a RawImage
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        protected override IRawImage LoadImage(System.Drawing.Bitmap image)
        {
            var maxWidth = Math.Min(Width, image.Width);
            var maxHeight = Math.Min(Height, image.Height);

            return new BitmapRawImage(image, maxWidth, maxHeight);
        }

        #endregion Protected Methods

        //########################################################################################

    }
}