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

using Waveshare.Common;

namespace Waveshare.Interfaces
{
    /// <summary>
    /// Internal Interface to initialize with the Hardware Interface for GPIO and SPI Bus
    /// </summary>
    internal interface IEPaperDisplayInternal : IEPaperDisplay
    {
        /// <summary>
        /// Color Bytes per Pixel (R, G, B)
        /// </summary>
        int ColorBytesPerPixel { get; set; }

        /// <summary>
        /// Supported Colors of the E-Paper Device
        /// </summary>
        ByteColor[] SupportedByteColors { get; }

        /// <summary>
        /// E-Paper Hardware Interface for GPIO and SPI Bus
        /// </summary>
        IEPaperDisplayHardware EPaperDisplayHardware { get; set; }

        /// <summary>
        /// Initialize the Display with the Hardware Interface
        /// </summary>
        /// <param name="ePaperDisplayHardware">Hardware Interface for GPIO and SPI Bus</param>
        void Initialize(IEPaperDisplayHardware ePaperDisplayHardware);

        /// <summary>
        /// Display a Image on the Display
        /// </summary>
        /// <param name="rawImage">Bitmap that should be displayed</param>
        /// <param name="dithering">Use Dithering to display the image</param>
        void DisplayImage(IRawImage rawImage, bool dithering);
    }
}