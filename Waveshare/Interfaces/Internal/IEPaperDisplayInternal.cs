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

using System.IO;
using Waveshare.Common;

#endregion Usings

namespace Waveshare.Interfaces.Internal
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
        /// Color Bytes of the E-Paper Device corresponding to the supported colors
        /// </summary>
        byte[] DeviceByteColors { get; }

        /// <summary>
        /// Pixels per Byte on the Device
        /// </summary>
        int PixelPerByte { get; }

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

        /// <summary>
        /// Gets the index for the supported color closest to a color
        /// </summary>
        /// <param name="color">Color to look up</param>
        /// <returns>Color index of closest supported color</returns>
        int GetColorIndex(ByteColor color);

        /// <summary>
        /// Get a colored scan line on the device
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        byte[] GetColoredLineOnDevice(ByteColor rgb);

        /// <summary>
        /// Send a Command to the Display
        /// </summary>
        /// <param name="command"></param>
        void SendCommand(byte command);

        /// <summary>
        /// Send a stream to the Display
        /// </summary>
        /// <param name="stream"></param>
        void SendData(MemoryStream stream);
    }
}