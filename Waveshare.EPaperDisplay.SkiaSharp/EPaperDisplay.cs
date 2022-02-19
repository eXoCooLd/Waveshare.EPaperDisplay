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

using Waveshare.Devices;
using Waveshare.Interfaces;
using Waveshare.Image;

#endregion Usings

namespace Waveshare
{
    /// <summary>
    /// E-Paper Display Factory
    /// </summary>
    public static class EPaperDisplay
    {

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Create a instance of a E-Paper Display for System.Drawing.Bitmap
        /// </summary>
        /// <param name="displayType"></param>
        /// <returns></returns>
        public static IEPaperDisplaySKBitmap? Create(EPaperDisplayType displayType)
        {
            var ePaperDisplay = EPaperDisplayRaw.CreateEPaperDisplay(displayType);
            return ePaperDisplay != null ? new SKBitmapLoader(ePaperDisplay) : null;
        }

        #endregion Public Methods

        //########################################################################################

    }
}
