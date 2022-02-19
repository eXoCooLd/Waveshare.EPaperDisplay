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

#endregion Usings

namespace Waveshare.Devices.Epd5in65F
{
    /// <summary>
    /// Byte values for the supported hardware colors
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class Epd5in65fColors
    {
        /// <summary>
        /// Byte value for color black
        /// </summary>
        public const byte Black = 0x00;

        /// <summary>
        /// Byte value for color white
        /// </summary>
        public const byte White = 0x01;

        /// <summary>
        /// Byte value for color green
        /// </summary>
        public const byte Green = 0x02;

        /// <summary>
        /// Byte value for color blue
        /// </summary>
        public const byte Blue = 0x03;

        /// <summary>
        /// Byte value for color red
        /// </summary>
        public const byte Red = 0x04;

        /// <summary>
        /// Byte value for color yellow
        /// </summary>
        public const byte Yellow = 0x05;

        /// <summary>
        /// Byte value for color orange
        /// </summary>
        public const byte Orange = 0x06;

        /// <summary>
        /// Byte value for clear color
        /// </summary>
        public const byte Clean = 0x07;
    }
}
