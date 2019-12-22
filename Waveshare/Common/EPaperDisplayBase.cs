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
using System.Collections.Generic;
using System.Device.Gpio;
using System.Drawing;
using System.Threading;
using Waveshare.Interfaces;

#endregion Usings

namespace Waveshare.Common
{
    /// <summary>
    /// Base Class for all E-Paper Devices
    /// </summary>
    internal abstract class EPaperDisplayBase : IEPaperDisplayInternal
    {

        //########################################################################################

        #region Properties

        /// <summary>
        /// E-Paper Hardware Interface for GPIO and SPI Bus
        /// </summary>
        public IEPaperDisplayHardware EPaperDisplayHardware { get; set; }

        /// <summary>
        /// Pixel Width of the Display
        /// </summary>
        public abstract int Width { get; }

        /// <summary>
        /// Pixel Height of the Display
        /// </summary>
        public abstract int Height { get; }

        /// <summary>
        /// Get Status Command
        /// </summary>
        protected abstract byte GetStatusCommand { get; }

        /// <summary>
        /// Start Data Transmission Command
        /// </summary>
        protected abstract byte StartDataTransmissionCommand { get; }

        /// <summary>
        /// Stop Data Transmission Command
        /// </summary>
        protected abstract byte StopDataTransmissionCommand { get; }

        #endregion Properties

        //########################################################################################

        #region Constructor / Dispose / Finalizer

        /// <summary>
        /// Finalizer
        /// </summary>
        ~EPaperDisplayBase()
        {
            Dispose(false);

            EPaperDisplayHardware?.Dispose();
            EPaperDisplayHardware = null;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            EPaperDisplayHardware?.Dispose();
            EPaperDisplayHardware = null;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose for sub classes
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Wait until the display is ready
        /// </summary>
        public void WaitUntilReady()
        {
            bool busy;
            do
            {
                SendCommand(GetStatusCommand);
                busy = !(EPaperDisplayHardware.BusyPin == PinValue.High);
            } while (busy);
        }

        /// <summary>
        /// Reset the Display
        /// </summary>
        public void Reset()
        {
            EPaperDisplayHardware.ResetPin = PinValue.High;
            Thread.Sleep(200);
            EPaperDisplayHardware.ResetPin = PinValue.Low;
            Thread.Sleep(2);
            EPaperDisplayHardware.ResetPin = PinValue.High;
            Thread.Sleep(200);
        }

        /// <summary>
        /// Display a Image on the Display
        /// </summary>
        /// <param name="image">Bitmap that should be displayed</param>
        public void DisplayImage(Bitmap image)
        {
            var data = BitmapToData(image);

            SendCommand(StartDataTransmissionCommand);
            SendData(data);
            SendCommand(StopDataTransmissionCommand);

            TurnOnDisplay();
        }

        /// <summary>
        /// Initialize the Display with the Hardware Interface
        /// </summary>
        /// <param name="ePaperDisplayHardware"></param>
        public void Initialize(IEPaperDisplayHardware ePaperDisplayHardware)
        {
            EPaperDisplayHardware = ePaperDisplayHardware;

            DeviceInitialize();
        }

        /// <summary>
        /// Clear the Display
        /// </summary>
        public void Clear()
        {
            const int pixelPerByte = 2;
            var displayBytes = Width / pixelPerByte * Height;

            var pixelData = ColorToByte(Color.White);
            var twoWhitePixel = MergePixelDataInByte(pixelData, pixelData);

            SendCommand(StartDataTransmissionCommand);
            for (var i = 0; i < displayBytes; i++)
            {
                SendData(twoWhitePixel);
            }
            SendCommand(StopDataTransmissionCommand);

            TurnOnDisplay();
        }

        /// <summary>
        /// Send the Display into SleepMode
        /// </summary>
        public abstract void Sleep();

        #endregion Public Methods

        //########################################################################################

        #region Protected Methods

        /// <summary>
        /// Send a Command to the Display
        /// </summary>
        /// <param name="command"></param>
        protected void SendCommand(byte command)
        {
            EPaperDisplayHardware.SpiDcPin = PinValue.Low;
            EPaperDisplayHardware.SpiCsPin = PinValue.Low;
            EPaperDisplayHardware.WriteByte(command);
            EPaperDisplayHardware.SpiCsPin = PinValue.High;
        }

        /// <summary>
        /// Send one Data Byte to the Display
        /// </summary>
        /// <param name="data"></param>
        protected void SendData(byte data)
        {
            EPaperDisplayHardware.SpiDcPin = PinValue.High;
            EPaperDisplayHardware.SpiCsPin = PinValue.Low;
            EPaperDisplayHardware.WriteByte(data);
            EPaperDisplayHardware.SpiCsPin = PinValue.High;
        }

        /// <summary>
        /// Send a Data Array to the Display
        /// </summary>
        /// <param name="data"></param>
        protected void SendData(byte[] data)
        {
            foreach (var dataByte in data)
            {
                SendData(dataByte);
            }
        }

        /// <summary>
        /// Check if a Pixel is Monochrom (white, gray or black)
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns></returns>
        protected static bool IsMonochrom(Color pixel)
        {
            return pixel.R == pixel.G &&
                   pixel.R == pixel.B &&
                   pixel.G == pixel.B;
        }

        /// <summary>
        /// Device specific Initializer
        /// </summary>
        protected abstract void DeviceInitialize();

        /// <summary>
        /// Turn the Display On after a Sleep
        /// </summary>
        protected abstract void TurnOnDisplay();

        /// <summary>
        /// Convert a pixel to a DataByte
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns>Pixel converted to specific byte value for the hardware</returns>
        protected abstract byte ColorToByte(Color pixel);

        #endregion Protected Methods

        //########################################################################################

        #region Internal Methods

        /// <summary>
        /// Convert a Bitmap to a Byte Array
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        internal byte[] BitmapToData(Bitmap image)
        {
            const int pixelPerByte = 2;

            var imageData = new List<byte>();

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x += pixelPerByte)
                {
                    var pixel1 = GetPixel(image, x, y);
                    var pixel1Data = ColorToByte(pixel1);

                    var pixel2 = GetPixel(image, x + 1, y);
                    var pixel2Data = ColorToByte(pixel2);

                    var mergedData = MergePixelDataInByte(pixel1Data, pixel2Data);
                    imageData.Add(mergedData);
                }
            }

            return imageData.ToArray();
        }

        /// <summary>
        /// Get the Pixel at position x, y or return a white pixel if it is out of bounds
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal static Color GetPixel(Bitmap image, int x, int y)
        {
            var color = Color.White;

            if (x < image.Width && y < image.Height)
            {
                color = image.GetPixel(x, y);
            }

            return color;
        }

        /// <summary>
        /// Merge two DataBytes into one Byte
        /// </summary>
        /// <param name="pixel1"></param>
        /// <param name="pixel2"></param>
        /// <returns></returns>
        internal static byte MergePixelDataInByte(byte pixel1, byte pixel2)
        {
            var output = (byte) (pixel1 << 4);
            output |= pixel2;
            return output;
        }

        #endregion Internal Methods

        //########################################################################################

    }
}