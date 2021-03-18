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
using System.Device.Gpio;
#endregion Usings

namespace Waveshare.Interfaces
{
    /// <summary>
    /// E-Paper Hardware Interface for GPIO and SPI Bus
    /// </summary>
    internal interface IEPaperDisplayHardware : IDisposable
    {
        /// <summary>
        /// GPIO Reset Pin
        /// </summary>
        PinValue ResetPin { get; set; }

        /// <summary>
        /// GPIO SPI DC Pin
        /// </summary>
        PinValue SpiDcPin { get; set; }

        /// <summary>
        /// GPIO SPI CS Pin
        /// </summary>
        PinValue SpiCsPin { get; set; }

        /// <summary>
        /// GPIO Busy Pin
        /// </summary>
        PinValue BusyPin { get; set; }

        /// <summary>
        /// Write data to the SPI device
        /// </summary>
        /// <param name="buffer">The buffer that contains the data to be written to the SPI device</param>
        void Write(byte[] buffer);

        /// <summary>
        /// Write a byte to the SPI device
        /// </summary>
        /// <param name="value">The byte to be written to the SPI device</param>
        void WriteByte(byte value);
    }
}