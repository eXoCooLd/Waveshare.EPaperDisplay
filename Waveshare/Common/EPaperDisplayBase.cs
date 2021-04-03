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
using System.Collections.ObjectModel;
using System.Device.Gpio;
using System.Diagnostics;
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

        #region Constants

        /// <summary>
        /// Timeout for the device Wait Until Ready
        /// </summary>
        private const int WaitUntilReadyTimeout = 50000;

        #endregion Constants

        //########################################################################################

        #region Fields

        /// <summary>
        /// A white scan line on the device
        /// </summary>
        private ReadOnlyCollection<byte> m_WhiteLineOnDevice;

        /// <summary>
        /// Byte value for a block (PixelPerByte) of white pixels on the Device
        /// </summary>
        private byte? m_WhitePixelBlockOnDevice;

        #endregion Fields

        //########################################################################################

        #region Properties

        /// <summary>
        /// Pixel Width of the Display
        /// </summary>
        public abstract int Width { get; }

        /// <summary>
        /// Pixel Height of the Display
        /// </summary>
        public abstract int Height { get; }

        /// <summary>
        /// Color Bytes per Pixel (R, G, B)
        /// </summary>
        public int ColorBytesPerPixel { get; set; } = 3;

        /// <summary>
        /// Default white scan line on the device
        /// </summary>
        private ReadOnlyCollection<byte> WhiteLineOnDevice => m_WhiteLineOnDevice ?? (m_WhiteLineOnDevice = GetWhiteLineOnDevice());

        /// <summary>
        /// Byte value for a block (PixelPerByte) of white pixels on the Device
        /// </summary>
        protected byte WhitePixelBlockOnDevice
        {
            get
            {
                if (!m_WhitePixelBlockOnDevice.HasValue)
                {
                    m_WhitePixelBlockOnDevice = GetMergedPixelDataInByte(ByteColors.White);
                }

                return m_WhitePixelBlockOnDevice.Value;
            }
        }

        /// <summary>
        /// Pixels per Byte on the Device
        /// </summary>
        protected abstract int PixelPerByte { get; }

        /// <summary>
        /// E-Paper Hardware Interface for GPIO and SPI Bus
        /// </summary>
        public IEPaperDisplayHardware EPaperDisplayHardware { get; set; }

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

            DeviceShutdown();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            DeviceShutdown();

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
        /// <returns>true if device is ready, false for timeout</returns>
        public bool WaitUntilReady()
        {
            return WaitUntilReady(WaitUntilReadyTimeout);
        }

        /// <summary>
        /// Wait until the display is ready
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>true if device is ready, false for timeout</returns>
        public bool WaitUntilReady(int timeout)
        {
            bool busy;

            var timeoutTimer = Stopwatch.StartNew();
            do
            {
                SendCommand(GetStatusCommand);
                busy = !(EPaperDisplayHardware.BusyPin == PinValue.High);

                if (timeoutTimer.ElapsedMilliseconds > timeout)
                {
                    return false;
                }
            } while (busy);

            return true;
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
        /// <param name="rawImage">Bitmap that should be displayed</param>
        public void DisplayImage(IRawImage rawImage)
        {
            SendCommand(StartDataTransmissionCommand);

            SendBitmapToDevice(rawImage.ScanLine, rawImage.Stride, rawImage.Width, rawImage.Height);

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
        /// Clear the Display to White
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Clear the Display to Black
        /// </summary>
        public abstract void ClearBlack();

        /// <summary>
        /// Power the controller on.  Do not use with SleepMode.
        /// </summary>
        public abstract void PowerOn();

        /// <summary>
        /// Power the controller off.  Do not use with SleepMode.
        /// </summary>
        public abstract void PowerOff();

        /// <summary>
        /// Send the Display into SleepMode
        /// </summary>
        public abstract void Sleep();

        /// <summary>
        /// WakeUp the Display from SleepMode
        /// </summary>
        public void WakeUp()
        {
            DeviceInitialize();
        }

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
            EPaperDisplayHardware.SpiDcPin = PinValue.High;
            EPaperDisplayHardware.SpiCsPin = PinValue.Low;
            EPaperDisplayHardware.Write(data);
            EPaperDisplayHardware.SpiCsPin = PinValue.High;
        }

        /// <summary>
        /// Check if a Pixel is Monochrom (white, gray or black)
        /// </summary>
        /// <param name="r">Red color byte</param>
        /// <param name="g">Green color byte</param>
        /// <param name="b">Blue color byte</param>
        /// <returns></returns>
        protected static bool IsMonochrom(byte r, byte g, byte b)
        {
            return r == g && g == b;
        }

        /// <summary>
        /// Get a colored scan line on the device
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        protected byte[] GetColoredLineOnDevice(byte r, byte g, byte b)
        {
            return GetColoredLineOnDevice(new []{r, g, b});
        }


        /// <summary>
        /// Get a colored scan line on the device
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        protected byte[] GetColoredLineOnDevice(byte[] rgb)
        {
            var devicePixel = GetMergedPixelDataInByte(rgb);

            var outputWidth = Width / PixelPerByte;
            var outputLine = new byte[outputWidth];

            for (var x = 0; x < outputLine.Length; x++)
            {
                outputLine[x] = devicePixel;
            }

            return outputLine;
        }

        /// <summary>
        /// Clone the default white scan line into a new byte array
        /// </summary>
        /// <returns></returns>
        protected byte[] CloneWhiteScanLine()
        {
            var scanLine = new byte[WhiteLineOnDevice.Count];
            WhiteLineOnDevice.CopyTo(scanLine, 0);
            return scanLine;
        }

        /// <summary>
        /// Get Pixel from the byte array
        /// </summary>
        /// <param name="line"></param>
        /// <param name="xPosition"></param>
        /// <param name="pixel"></param>
        /// <returns></returns>
        protected byte GetPixelFromArray(byte[] line, int xPosition, int pixel)
        {
            var pixelWith = ColorBytesPerPixel * pixel;

            var colorR = xPosition + pixelWith + 2;
            var colorG = xPosition + pixelWith + 1;
            var colorB = xPosition + pixelWith + 0;

            if (colorR >= line.Length)
            {
                return WhitePixelBlockOnDevice;
            }

            return ColorToByte(line[colorR], line[colorG], line[colorB]);
        }

        /// <summary>
        /// Get the two pixel from the array and merge them into a device pixel
        /// </summary>
        /// <param name="xPosition"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        protected byte GetDevicePixels(int xPosition, byte[] line)
        {
            var pixels = new byte[PixelPerByte];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = GetPixelFromArray(line, xPosition, i);
            }

            return MergePixelDataInByte(pixels);
        }

        /// <summary>
        /// Device specific Initializer
        /// </summary>
        protected abstract void DeviceInitialize();

        /// <summary>
        /// Turn the Display PowerOn after a Sleep
        /// </summary>
        protected abstract void TurnOnDisplay();

        /// <summary>
        /// Convert a pixel to a DataByte
        /// </summary>
        /// <param name="r">Red color byte</param>
        /// <param name="g">Green color byte</param>
        /// <param name="b">Blue color byte</param>
        /// <returns>Pixel converted to specific byte value for the hardware</returns>
        protected abstract byte ColorToByte(byte r, byte g, byte b);

        #endregion Protected Methods

        //########################################################################################

        #region Internal Methods

        /// <summary>
        /// Merge pixels into a Device Byte
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns></returns>
        internal byte MergePixelDataInByte(params byte[] pixel)
        {
            if (pixel == null || pixel.Length == 0)
            {
                throw new ArgumentException($"Argument {nameof(pixel)} can not be null or empty", nameof(pixel));
            }

            if (pixel.Length != PixelPerByte)
            {
                throw new ArgumentException($"Argument {nameof(pixel)}.Length is not PixelPerByte {PixelPerByte}", nameof(pixel));
            }

            const int bitsInByte = 8;
            var bitMoveLength = bitsInByte / PixelPerByte;

            byte output = 0;

            for (var i = 0; i < pixel.Length; i++)
            {
                var moveFactor = bitsInByte - bitMoveLength * (i + 1);
                output |= (byte)(pixel[i] << moveFactor);
            }

            return output;
        }

        /// <summary>
        /// Send a Bitmap as Byte Array to the Device
        /// </summary>
        /// <param name="scanLine">Int Pointer to the start of the Bytearray</param>
        /// <param name="stride">Length of a ScanLine</param>
        /// <param name="maxX">Max Pixels horizontal</param>
        /// <param name="maxY">Max Pixels Vertical</param>
        internal abstract void SendBitmapToDevice(IntPtr scanLine, int stride, int maxX, int maxY);

        #endregion Internal Methods

        //########################################################################################

        #region Private Methods

        /// <summary>
        /// Shut down the device
        /// </summary>
        private void DeviceShutdown()
        {
            if (EPaperDisplayHardware != null)
            {
                Sleep();

                EPaperDisplayHardware?.Dispose();
                EPaperDisplayHardware = null;
            }
        }
        /// <summary>
        /// Return a white scan line for the device
        /// </summary>
        /// <returns></returns>
        private ReadOnlyCollection<byte> GetWhiteLineOnDevice()
        {
            return new ReadOnlyCollection<byte>(GetColoredLineOnDevice(ByteColors.White));
        }

        /// <summary>
        /// Get a byte on the device with one Color for all pixels
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        private byte GetMergedPixelDataInByte(byte[] rgb)
        {
            if (rgb == null || rgb.Length > 3)
            {
                throw new ArgumentException($"The Bytearray {nameof(rgb)} can not be null and its length has to be {ColorBytesPerPixel} or grater but is {rgb?.Length}");
            }

            var pixelData = ColorToByte(rgb[0], rgb[1], rgb[2]);
            var deviceBytesPerPixel = new byte[PixelPerByte];

            for (var i = 0; i < deviceBytesPerPixel.Length; i++)
            {
                deviceBytesPerPixel[i] = pixelData;
            }

            return MergePixelDataInByte(deviceBytesPerPixel);
        }

        #endregion Private Methods

        //########################################################################################

    }
}