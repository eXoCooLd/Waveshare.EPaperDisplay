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
using Waveshare.Common;
using Waveshare.Devices;
using Waveshare.Devices.Epd7in5_V2;
using Waveshare.Devices.Epd7in5b_V2;
using Waveshare.Devices.Epd7in5bc;
using Waveshare.Image.Bitmap;
using Waveshare.Interfaces;
using Waveshare.Interfaces.Internal;

#endregion Usings

namespace Waveshare
{
    /// <summary>
    /// E-Paper Display Factory
    /// </summary>
    public static class EPaperDisplay
    {

        //########################################################################################

        #region Properties

        /// <summary>
        /// E-Paper Hardware Interface for GPIO and SPI Bus
        /// </summary>
        internal static Lazy<IEPaperDisplayHardware> EPaperDisplayHardware { get; set; } = new Lazy<IEPaperDisplayHardware>(() => new EPaperDisplayHardware());

        #endregion Properties

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Create a instance of a E-Paper Display
        /// </summary>
        /// <param name="displayType"></param>
        /// <returns></returns>
        public static IEPaperDisplayBitmap Create(EPaperDisplayType displayType)
        {
            var ePaperDisplay = CreateEPaperDisplay(displayType);
            return ePaperDisplay != null ? new BitmapLoader(ePaperDisplay) : null;
        }

        #endregion Public Methods

        //########################################################################################

        #region Private Methods

        /// <summary>
        /// Create a instance of a internal E-Paper Display
        /// </summary>
        /// <param name="displayType"></param>
        /// <returns></returns>
        private static IEPaperDisplayInternal CreateEPaperDisplay(EPaperDisplayType displayType)
        {
            IEPaperDisplayInternal display;

            switch (displayType)
            {
                case EPaperDisplayType.WaveShare7In5Bc:
                    display = new Epd7In5Bc();
                    break;
                case EPaperDisplayType.WaveShare7In5_V2:
                    display = new Epd7In5_V2();
                    break;
                case EPaperDisplayType.WaveShare7In5b_V2:
                    display = new Epd7In5b_V2();
                    break;
                default:
                    display = null;
                    break;
            }

            display?.Initialize(EPaperDisplayHardware.Value);

            return display;
        }

        #endregion Private Methods

        //########################################################################################

    }
}