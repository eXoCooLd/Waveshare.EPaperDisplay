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
using Waveshare.Image.SKBitmap;
using Waveshare.Interfaces;
using Waveshare.Interfaces.Internal;

#endregion Usings

namespace Waveshare.Image.SkBitmap
{
    internal class SKBitmapLoader : EPaperImageBase<SkiaSharp.SKBitmap>, IEPaperDisplaySKBitmap
    {
        //########################################################################################

        #region Constructor / Dispose / Finalizer

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ePaperDisplay"></param>
        public SKBitmapLoader(IEPaperDisplayInternal ePaperDisplay) : base(ePaperDisplay)
        {
        }

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

        #region Protected Methods

        /// <summary>
        /// Load the SSKBitmap into a RawImage
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        protected override IRawImage LoadImage(SkiaSharp.SKBitmap image)
        {
            var maxWith = Math.Min(Width, image.Width);
            var maxHeight = Math.Min(Height, image.Height);

            var subSet = new SkiaSharp.SKBitmap();
            image.ExtractSubset(subSet, new SkiaSharp.SKRectI(0, 0, maxWith, maxHeight));

            return new SKBitmapRawImage(image.Resize(new SkiaSharp.SKImageInfo(maxWith, maxHeight, SkiaSharp.SKColorType.Bgra8888), SkiaSharp.SKFilterQuality.High));
        }

        #endregion Protected Methods

        //########################################################################################
    }
}
