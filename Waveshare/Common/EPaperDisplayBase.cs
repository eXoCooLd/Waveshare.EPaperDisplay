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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
        /// Has class been disposed
        /// </summary>
        private bool m_Disposed;

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
        /// Supported Colors of the E-Paper Device
        /// </summary>
        public abstract ByteColor[] SupportedByteColors { get; }

        /// <summary>
        /// Color Bytes of the E-Paper Device corresponding to the supported colors
        /// </summary>
        public abstract byte[] DeviceByteColors { get; }

        /// <summary>
        /// Color Bytes per Pixel (R, G, B)
        /// </summary>
        public int ColorBytesPerPixel { get; set; } = 3;

        /// <summary>
        /// Color Bytes per Pixel (R, G, B)
        /// </summary>
        public bool IsPalletMonochrome { get; private set; }

        /// <summary>
        /// Display Writer assigned to the device
        /// </summary>
        public abstract EPaperDisplayWriter DisplayWriter { get; }

        /// <summary>
        /// Pixels per Byte on the Device
        /// </summary>
        public abstract int PixelPerByte { get; }

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
        ~EPaperDisplayBase() => Dispose(false);

        /// <summary>
        /// Dispose of class
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of instantiated objects
        /// </summary>
        /// <param name="disposing">Explicit dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_Disposed)
            {
                return;
            }

            if (disposing)
            {
                DisplayWriter?.Dispose();
                DeviceShutdown();
            }

            m_Disposed = true;
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
        /// <param name="dithering"></param>
        public void DisplayImage(IRawImage rawImage, bool dithering)
        {
            SendCommand(StartDataTransmissionCommand);

            if (dithering)
            {
                SendDitheredBitmapToDevice(rawImage.ScanLine, rawImage.Stride, rawImage.Width, rawImage.Height);
            }
            else
            {
                SendBitmapToDevice(rawImage.ScanLine, rawImage.Stride, rawImage.Width, rawImage.Height);
            }

            if (StopDataTransmissionCommand < byte.MaxValue)
            {
                SendCommand(StopDataTransmissionCommand);
            }

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

            IsPalletMonochrome = true;
            foreach (var color in SupportedByteColors)
            {
                if (!color.IsMonochrome)
                {
                    IsPalletMonochrome = false;
                    break;
                }
            }
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

        /// <summary>
        /// Send a Command to the Display
        /// </summary>
        /// <param name="command"></param>
        public void SendCommand(byte command)
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
        public void SendData(byte data)
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
        public void SendData(byte[] data)
        {
            EPaperDisplayHardware.SpiDcPin = PinValue.High;
            EPaperDisplayHardware.SpiCsPin = PinValue.Low;
            EPaperDisplayHardware.Write(data);
            EPaperDisplayHardware.SpiCsPin = PinValue.High;
        }

        /// <summary>
        /// Send a stream to the Display
        /// </summary>
        /// <param name="stream"></param>
        public void SendData(MemoryStream stream)
        {
            EPaperDisplayHardware.SpiDcPin = PinValue.High;
            EPaperDisplayHardware.SpiCsPin = PinValue.Low;
            EPaperDisplayHardware.Write(stream);
            EPaperDisplayHardware.SpiCsPin = PinValue.High;
        }

        /// <summary>
        /// Get a colored scan line on the device
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        public byte[] GetColoredLineOnDevice(ByteColor rgb)
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
        /// Gets the index for the supported color closest to a color
        /// </summary>
        /// <param name="color">Color to look up</param>
        /// <returns>Color index of closest supported color</returns>
        public int GetColorIndex(ByteColor color)
        {
            var minDistance = GetColorDistance(color, SupportedByteColors[0]);
            if (minDistance < 1)
            {
                return 0;
            }

            var bestIndex = 0;

            for (var i = 1; i < SupportedByteColors.Length; i++)
            {
                var distance = GetColorDistance(color, SupportedByteColors[i]);
                if (distance <= minDistance)
                {
                    minDistance = distance;
                    bestIndex = i;
                    if (minDistance < 1)
                    {
                        break;
                    }
                }
            }

            return bestIndex;
        }

        #endregion Public Methods

        //########################################################################################

        #region Protected Methods

        /// <summary>
        /// Gets the Euclidean distance between two colors
        /// </summary>
        /// <param name="color1">First color to compare</param>
        /// <param name="color2">Second color to compare</param>
        /// <returns>Returns the distance between two colors</returns>
        protected double GetColorDistance(ByteColor color1, ByteColor color2)
        {
            if (IsPalletMonochrome)
            {
                return (color1.R - color2.R) * (color1.R - color2.R);
            }

            var (y1, u1, v1) = GetYuv(color1);
            var (y2, u2, v2) = GetYuv(color2);
            var diffY = y1 - y2;
            var diffU = u1 - u2;
            var diffV = v1 - v2;

            return diffY * diffY + diffU * diffU + diffV * diffV;
        }

        /// <summary>
        /// Adjust RGB values on a color. Clamping the values between 0 and 255.
        /// </summary>
        /// <param name="color">Color to adjust</param>
        /// <param name="valueR">Red value to add</param>
        /// <param name="valueG">Green value to add</param>
        /// <param name="valueB">Blue value to add</param>
        protected static void AdjustRgb(ref ByteColor color, int valueR, int valueG, int valueB)
        {
            var r = ConvertColorIntToByte(color.R + valueR);
            var g = ConvertColorIntToByte(color.G + valueG);
            var b = ConvertColorIntToByte(color.B + valueB);

            color.SetBGR(b, g, r);
        }

        /// <summary>
        /// Floyd-Steinberg Dithering
        /// </summary>
        /// <param name="data"></param>
        /// <param name="previousLine"></param>
        /// <param name="currentLine"></param>
        /// <param name="isLastLine"></param>
        protected void DitherAndWrite(ByteColor[,] data, int previousLine, int currentLine, bool isLastLine)
        {
            for (var x = 0; x < Width; x++)
            {
                var currentPixel = data[x, previousLine];
                var colorNdx = GetColorIndex(currentPixel);
                var bestColor = SupportedByteColors[colorNdx];

                var errorR = currentPixel.R - bestColor.R;
                var errorG = currentPixel.G - bestColor.G;
                var errorB = currentPixel.B - bestColor.B;

                // Add 7/16 of the color difference to the pixel on the right
                if (x < Width - 1)
                {
                    AdjustRgb(ref data[x + 1, previousLine], errorR * 7 / 16, errorG * 7 / 16, errorB * 7 / 16);
                }

                if (!isLastLine)
                {
                    // Add 3/16 of the color difference to the pixel below and to the left
                    if (x > 0)
                    {
                        AdjustRgb(ref data[x - 1, currentLine], errorR * 3 / 16, errorG * 3 / 16, errorB * 3 / 16);
                    }

                    // Add 5/16 of the color difference to the pixel directly below
                    AdjustRgb(ref data[x, currentLine], errorR * 5 / 16, errorG * 5 / 16, errorB * 5 / 16);

                    // Add 1/16 of the color difference to the pixel below and to the right
                    if (x < Width - 1)
                    {
                        AdjustRgb(ref data[x + 1, currentLine], errorR / 16, errorG / 16, errorB / 16);
                    }
                }

                DisplayWriter.Write(colorNdx);
            }
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
        /// <param name="rgb">color byte</param>
        /// <returns>Pixel converted to specific byte value for the hardware</returns>
        protected abstract byte ColorToByte(ByteColor rgb);

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

            const int bitStates = 2;
            const int bitsInByte = 8;

            var bitMoveLength = bitsInByte / PixelPerByte;
            var maxValue = (byte) Math.Pow(bitStates, bitMoveLength) - 1;

            byte output = 0;

            for (var i = 0; i < pixel.Length; i++)
            {
                var bitMoveValue = bitsInByte - bitMoveLength - (i * bitMoveLength); 

                var value = (byte)(pixel[i] << bitMoveValue);
                var mask = maxValue << bitMoveValue;
                var posValue = (byte)(value & mask);

                output |= posValue;
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
        internal void SendBitmapToDevice(IntPtr scanLine, int stride, int maxX, int maxY)
        {
            var inputLine = new byte[stride];
            var pixel = new ByteColor(0, 0, 0);

            for (var y = 0; y < maxY; y++, scanLine += stride)
            {
                Marshal.Copy(scanLine, inputLine, 0, inputLine.Length);

                var xPos = 0;
                for (var x = 0; x < maxX; x++)
                {
                    pixel.SetBGR(inputLine[xPos++], inputLine[xPos++], inputLine[xPos++], IsPalletMonochrome);
                    DisplayWriter.Write(GetColorIndex(pixel));
                }

                for (var x = maxX; x < Width; x++)
                {
                    DisplayWriter.WriteBlankPixel();
                }
            }

            // Write blank lines if image is smaller than display.
            for (var y = maxY; y < Height; y++)
            {
                DisplayWriter.WriteBlankLine();
            }

            DisplayWriter.Finish();
        }

        /// <summary>
        /// Send a Dithered Bitmap as Byte Array to the Device
        /// </summary>
        /// <param name="scanLine">Int Pointer to the start of the Bytearray</param>
        /// <param name="stride">Length of a ScanLine</param>
        /// <param name="maxX">Max Pixels horizontal</param>
        /// <param name="maxY">Max Pixels Vertical</param>
        internal void SendDitheredBitmapToDevice(IntPtr scanLine, int stride, int maxX, int maxY)
        {
            var data = new ByteColor[Width, 2];
            var currentLine = 0;
            var previousLine = 1;

            var inputLine = new byte[stride];
            var pixel = new ByteColor(0, 0, 0);
            var odd = false;
            var dither = false;

            for (var y = 0; y < maxY; y++, scanLine += stride)
            {
                if (odd)
                {
                    previousLine = 0;
                    currentLine = 1;
                    odd = false;
                    dither = true;
                }
                else
                {
                    previousLine = 1;
                    currentLine = 0;
                    odd = true;
                }

                Marshal.Copy(scanLine, inputLine, 0, inputLine.Length);

                var xPos = 0;
                for (var x = 0; x < maxX; x++)
                {
                    pixel.SetBGR(inputLine[xPos++], inputLine[xPos++], inputLine[xPos++], IsPalletMonochrome);
                    data[x, currentLine] = pixel;
                }

                for (var x = maxX; x < Width; x++)
                {
                    data[x, currentLine] = ByteColors.White;
                }

                if (dither)
                {
                    DitherAndWrite(data, previousLine, currentLine, false);
                }
            }

            // Finish last line
            DitherAndWrite(data, currentLine, previousLine, true);

            // Write blank lines if image is smaller than display.
            for (var y = maxY; y < Height; y++)
            {
                DisplayWriter.WriteBlankLine();
            }

            DisplayWriter.Finish();
        }

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
        /// Get a byte on the device with one Color for all pixels
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        private byte GetMergedPixelDataInByte(ByteColor rgb)
        {
            var pixelData = ColorToByte(rgb);
            var deviceBytesPerPixel = new byte[PixelPerByte];

            for (var i = 0; i < deviceBytesPerPixel.Length; i++)
            {
                deviceBytesPerPixel[i] = pixelData;
            }

            return MergePixelDataInByte(deviceBytesPerPixel);
        }

        /// <summary>
        /// Calculate YUV color space
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private static (double Y, double U, double V) GetYuv(ByteColor color)
        {
            return (color.R * .299000 + color.G * .587000 + color.B * .114000,
                color.R * -.168736 + color.G * -.331264 + color.B * .500000 + 128,
                color.R * .500000 + color.G * -.418688 + color.B * -.081312 + 128);
        }

        /// <summary>
        /// Convert a Color Int to a Color Byte
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private static byte ConvertColorIntToByte(int level)
        {
            if (level < byte.MinValue)
            {
                return byte.MinValue;
            }
            
            if (level > byte.MaxValue)
            {
                return byte.MaxValue;
            }

            return (byte)level;
        }

        #endregion Private Methods

        //########################################################################################

    }
}
