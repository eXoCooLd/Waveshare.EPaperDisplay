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
        /// A white scan line on the device
        /// </summary>
        private ReadOnlyCollection<byte> m_WhiteLineOnDevice;

        /// <summary>
        /// Byte value for a block (PixelPerByte) of white pixels on the Device
        /// </summary>
        private byte? m_WhitePixelBlockOnDevice;

        /// <summary>
        /// Has class been disposed
        /// </summary>
        private bool Disposed = false;

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
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                if (DisplayWriter != null)
                {
                    DisplayWriter.Dispose();
                }

                DeviceShutdown();
            }

            Disposed = true;
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
            foreach (ByteColor color in SupportedByteColors)
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

        #endregion Public Methods

        //########################################################################################

        #region Protected Methods

        /// <summary>
        /// Send a Command to the Display
        /// </summary>
        /// <param name="command"></param>
        internal void SendCommand(byte command)
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
        protected internal void SendData(byte[] data)
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
        protected internal void SendData(MemoryStream stream)
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
        protected internal byte[] GetColoredLineOnDevice(ByteColor rgb)
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

            return ColorToByte(new ByteColor(line[colorR], line[colorG], line[colorB]));
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
        /// Gets the index for the supported color closest to a color
        /// </summary>
        /// <param name="color">Color to look up</param>
        /// <returns>Color index of closest supported color</returns>
        protected internal byte GetColorIndex(ByteColor color)
        {
            double minDistance = GetColorDistance(color, SupportedByteColors[0]);
            byte bestIndex = 0;
            if (minDistance > 0)
            {
                for (byte i = 1; i < SupportedByteColors.Length; i++)
                {
                    double distance = GetColorDistance(color, SupportedByteColors[i]);
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
            }

            return bestIndex;
        }

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

            (double Y1, double U1, double V1) = GetYUV(color1);
            (double Y2, double U2, double V2) = GetYUV(color2);
            double diffY = Y1 - Y2;
            double diffU = U1 - U2;
            double diffV = V1 - V2;

            return diffY * diffY + diffU * diffU + diffV * diffV;
        }

        /// <summary>
        /// Calculate YUV color space
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private (double Y, double U, double V) GetYUV(ByteColor color)
        {
            return (color.R * .299000 + color.G * .587000 + color.B * .114000,
                    color.R * -.168736 + color.G * -.331264 + color.B * .500000 + 128,
                    color.R * .500000 + color.G * -.418688 + color.B * -.081312 + 128);
        }

        /// <summary>
        /// Adjust RGB values on a color. Clamping the values between 0 and 255.
        /// </summary>
        /// <param name="color">Color to adjust</param>
        /// <param name="valueR">Red value to add</param>
        /// <param name="valueG">Green value to add</param>
        /// <param name="valueB">Blue value to add</param>
        protected static void AdjustRGB(ref ByteColor color, int valueR, int valueG, int valueB)
        {
            byte R, G, B;
            int level = color.R + valueR;
            if (level < 0)
            {
                R = 0;
            }
            else if (level > 255)
            {
                R = 255;
            }
            else
            {
                R = (byte)level;
            }

            level = color.G + valueG;
            if (level < 0)
            {
                G = 0;
            }
            else if (level > 255)
            {
                G = 255;
            }
            else
            {
                G = (byte)level;
            }

            level = color.B + valueB;
            if (level < 0)
            {
                B = 0;
            }
            else if (level > 255)
            {
                B = 255;
            }
            else
            {
                B = (byte)level;
            }
            color.SetBGR(B, G, R);
        }

        /// Floyd-Steinberg Dithering
        protected void DitherAndWrite(ByteColor[,] data, int previousLine, int currentLine, bool isLastLine)
        {
            for (int x = 0; x < Width; x++)
            {
                ByteColor currentPixel = data[x, previousLine];
                byte colorNdx = GetColorIndex(currentPixel);
                ByteColor bestColor = SupportedByteColors[colorNdx];

                int errorR = currentPixel.R - bestColor.R;
                int errorG = currentPixel.G - bestColor.G;
                int errorB = currentPixel.B - bestColor.B;

                // Add 7/16 of the color difference to the pixel on the right
                if (x < Width - 1)
                {
                    AdjustRGB(ref data[x + 1, previousLine], errorR * 7 / 16, errorG * 7 / 16, errorB * 7 / 16);
                }

                if (!isLastLine)
                {
                    // Add 3/16 of the color difference to the pixel below and to the left
                    if (x > 0)
                    {
                        AdjustRGB(ref data[x - 1, currentLine], errorR * 3 / 16, errorG * 3 / 16, errorB * 3 / 16);
                    }

                    // Add 5/16 of the color difference to the pixel directly below
                    AdjustRGB(ref data[x, currentLine], errorR * 5 / 16, errorG * 5 / 16, errorB * 5 / 16);

                    // Add 1/16 of the color difference to the pixel below and to the right
                    if (x < Width - 1)
                    {
                        AdjustRGB(ref data[x + 1, currentLine], errorR / 16, errorG / 16, errorB / 16);
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
        internal virtual void SendBitmapToDevice(IntPtr scanLine, int stride, int maxX, int maxY)
        {
            byte[] inputLine = new byte[stride];
            ByteColor pixel = new ByteColor(0, 0, 0);

            for (int y = 0; y < maxY; y++, scanLine += stride)
            {
                Marshal.Copy(scanLine, inputLine, 0, inputLine.Length);

                int xpos = 0;
                for (int x = 0; x < maxX; x++)
                {
                    pixel.SetBGR(inputLine[xpos++], inputLine[xpos++], inputLine[xpos++], IsPalletMonochrome);
                    DisplayWriter.Write(GetColorIndex(pixel));
                }

                for (int x = maxX; x < Width; x++)
                {
                    DisplayWriter.WriteBlankPixel();
                }
            }

            // Write blank lines if image is smaller than display.
            for (int y = maxY; y < Height; y++)
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
        internal virtual void SendDitheredBitmapToDevice(IntPtr scanLine, int stride, int maxX, int maxY)
        {
            ByteColor[,] data = new ByteColor[Width, 2];
            int currentLine = 0;
            int previousLine = 1;

            byte[] inputLine = new byte[stride];
            ByteColor pixel = new ByteColor(0, 0, 0);
            bool odd = false;
            bool dither = false;

            for (int y = 0; y < maxY; y++, scanLine += stride)
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

                int xpos = 0;
                for (int x = 0; x < maxX; x++)
                {
                    pixel.SetBGR(inputLine[xpos++], inputLine[xpos++], inputLine[xpos++], IsPalletMonochrome);
                    data[x, currentLine] = pixel;
                }

                for (int x = maxX; x < Width; x++)
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
            for (int y = maxY; y < Height; y++)
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

        #endregion Private Methods

        //########################################################################################
    }
}