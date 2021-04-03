#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2020 Greg Cannon

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
using System.Runtime.InteropServices;
using System.Threading;
using Waveshare.Common;

#endregion Usings

namespace Waveshare.Devices.Epd7in5_V2
{
    /// <summary>
    /// Type: Waveshare 7.5inch e-Paper V2
    /// Color: Black and White
    /// Display Resolution: 800*480
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal sealed class Epd7In5_V2 : EPaperDisplayBase
    {

        //########################################################################################

        #region Properties

        /// <summary>
        /// Pixels per Byte on the Device
        /// </summary>
        protected override int PixelPerByte { get; } = 8;

        /// <summary>
        /// Pixel Width of the Display
        /// </summary>
        public override int Width { get; } = 800;

        /// <summary>
        /// Pixel Height of the Display
        /// </summary>
        public override int Height { get; } = 480;

        /// <summary>
        /// Get Status Command
        /// </summary>
        protected override byte GetStatusCommand { get; } = (byte)Epd7In5_V2Commands.GetStatus;

        /// <summary>
        /// Start Data Transmission Command
        /// </summary>
        protected override byte StartDataTransmissionCommand { get; } = (byte)Epd7In5_V2Commands.DataStartTransmission2;

        /// <summary>
        /// Stop Data Transmission Command
        /// </summary>
        protected override byte StopDataTransmissionCommand { get; } = (byte)Epd7In5_V2Commands.DataStop;

        #endregion Properties

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Clear the Display to White
        /// </summary>
        public override void Clear()
        {
            FillColor(Epd7In5_V2Commands.DataStartTransmission1, ByteColors.White);
            FillColor(Epd7In5_V2Commands.DataStartTransmission2, ByteColors.White);
            TurnOnDisplay();
        }

        /// <summary>
        /// Clear the Display to Black
        /// </summary>
        public override void ClearBlack()
        {
            FillColor(Epd7In5_V2Commands.DataStartTransmission2, ByteColors.Black);
            TurnOnDisplay();
        }

        /// <summary>
        /// Power the controller on.  Do not use with SleepMode.
        /// </summary>
        public override void PowerOn()
        {
            SendCommand(Epd7In5_V2Commands.PowerOn);
            DeviceWaitUntilReady();
        }

        /// <summary>
        /// Power the controller off.  Do not use with SleepMode.
        /// </summary>
        public override void PowerOff()
        {
            SendCommand(Epd7In5_V2Commands.PowerOff);
            DeviceWaitUntilReady();
        }

        /// <summary>
        /// Send the Display into SleepMode
        /// </summary>
        public override void Sleep()
        {
            PowerOff();
            SendCommand(Epd7In5_V2Commands.DeepSleep);
            SendData(0xA5);
        }

        /// <summary>
        /// Wait until the display is ready
        /// </summary>
        public void DeviceWaitUntilReady()
        {
            WaitUntilReady();
            Thread.Sleep(200);
        }

        #endregion Public Methods

        //########################################################################################

        #region Protected Methods

        /// <summary>
        /// Device specific Initializer
        /// </summary>
        protected override void DeviceInitialize()
        {
            Reset();

            SendCommand(Epd7In5_V2Commands.BoosterSoftStart);
            SendData(0x17);
            SendData(0x17);
            SendData(0x27);
            SendData(0x17);

            SendCommand(Epd7In5_V2Commands.PowerSetting);
            SendData(0x07); // VGH: 20V
            SendData(0x17); // VGL: -20V
            SendData(0x3f); // VDH: 15V
            SendData(0x3f); // VDL: -15V

            SendCommand(Epd7In5_V2Commands.PowerOn);
            Thread.Sleep(100);
            DeviceWaitUntilReady();

            SendCommand(Epd7In5_V2Commands.PanelSetting);
            SendData(0x1F); // KW-3f   KWR-2F	BWROTP 0f	BWOTP 1f

            SendCommand(Epd7In5_V2Commands.TconResolution);
            SendData(0x03); // source 800
            SendData(0x20);
            SendData(0x01); // gate 480
            SendData(0xe0);

            SendCommand(Epd7In5_V2Commands.DualSpi);
            SendData(0x00);

            SendCommand(Epd7In5_V2Commands.VcomAndDataIntervalSetting);
            SendData(0x10);
            SendData(0x07);

            SendCommand(Epd7In5_V2Commands.TconSetting);
            SendData(0x22);
        }

        /// <summary>
        /// Turn the Display PowerOn after a Sleep
        /// </summary>
        protected override void TurnOnDisplay()
        {
            SendCommand(Epd7In5_V2Commands.DisplayRefresh);
            Thread.Sleep(100);
            DeviceWaitUntilReady();
        }

        /// <summary>
        /// Convert a pixel to a DataByte
        /// </summary>
        /// <param name="r">Red color byte</param>
        /// <param name="g">Green color byte</param>
        /// <param name="b">Blue color byte</param>
        /// <returns>Pixel converted to specific byte value for the hardware</returns>
        protected override byte ColorToByte(byte r, byte g, byte b)
        {
            if (r == g && r == b)
            {
                return (byte)(byte.MaxValue - r);
            }

            return (byte)(byte.MaxValue - (r * 0.299 + g * 0.587 + b * 0.114));
        }

        #endregion Protected Methods

        //########################################################################################

        #region Internal Methods

        /// <summary>
        /// Send a Bitmap as Byte Array to the Device
        /// </summary>
        /// <param name="scanLine">Int Pointer to the start of the Bytearray</param>
        /// <param name="stride">Length of a ScanLine</param>
        /// <param name="maxX">Max Pixels horizontal</param>
        /// <param name="maxY">Max Pixels Vertical</param>
        internal override void SendBitmapToDevice(IntPtr scanLine, int stride, int maxX, int maxY)
        {
            sbyte[,] data = new sbyte[maxX, 2];
            byte[] masks = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
            int outputStride = Width / PixelPerByte;
            byte[] output = new byte[outputStride];
            int lineC = 0;

            byte[] inputLine = new byte[stride];
            bool odd = false;
            bool dither = false;
            sbyte error;
            bool j;

            for (int y = 0; y < maxY; y++, scanLine += stride)
            {
                int lineP;
                if (odd)
                {
                    lineP = 0;
                    lineC = 1;
                    odd = false;
                    dither = true;
                }
                else
                {
                    lineP = 1;
                    lineC = 0;
                    odd = true;
                }
                Marshal.Copy(scanLine, inputLine, 0, inputLine.Length);

                // Convert to greyscale
                for (int x = 0; x < maxX; x++)
                {
                    var xpos = x * ColorBytesPerPixel;
                    data[x, lineC] = (sbyte)(64d * (ColorToByte(inputLine[xpos + 2], inputLine[xpos + 1], inputLine[xpos + 0]) / 255d - 0.5d));
                }

                if (dither)
                {
                    // Dither to monochrome 8 pixels per byte using Floyd-Steinberg
                    Array.Clear(output, 0, outputStride);
                    for (int x = 0; x < maxX; x++)
                    {
                        j = data[x, lineP] > 0;
                        if (j)
                        {
                            output[x / 8] |= masks[x % 8];
                        }

                        error = (sbyte)(data[x, lineP] - (j ? 32 : -32));
                        if (x < maxX - 1)
                        {
                            data[x + 1, lineP] += (sbyte)(7 * error / 16);
                        }

                        if (x > 0)
                        {
                            data[x - 1, lineC] += (sbyte)(3 * error / 16);
                        }

                        data[x, lineC] += (sbyte)(5 * error / 16);
                        if (x < maxX - 1)
                        {
                            data[x + 1, lineC] += (sbyte)(1 * error / 16);
                        }
                    }
                    SendData(output);
                }
            }

            Array.Clear(output, 0, outputStride);
            for (int x = 0; x < maxX; x++)
            {
                j = data[x, lineC] > 0;
                if (j)
                {
                    output[x / 8] |= masks[x % 8];
                }

                error = (sbyte)(data[x, lineC] - (j ? 32 : -32));
                if (x < maxX - 1)
                {
                    data[x + 1, lineC] += (sbyte)(7 * error / 16);
                }
            }

            SendData(output);
            if (maxY < Height)
            {
                Array.Clear(output, 0, outputStride);
                for (int y = maxY; y < Height; y++)
                {
                    SendData(output);
                }
            }
        }

        #endregion

        //########################################################################################

        #region Private Methods

        /// <summary>
        /// Helper to send a Command based o the Epd7In5_V2Commands Enum
        /// </summary>
        /// <param name="command">Command to send</param>
        private void SendCommand(Epd7In5_V2Commands command)
        {
            SendCommand((byte)command);
        }

        /// <summary>
        /// Fill the screen with a color
        /// </summary>
        /// <param name="command">Start Data Transmission Command</param>
        /// <param name="rgb">Color to fill the screen</param>
        private void FillColor(Epd7In5_V2Commands command, byte[] rgb)
        {
            var outputLine = GetColoredLineOnDevice(rgb);

            SendCommand(command);

            for (var y = 0; y < Height; y++)
            {
                SendData(outputLine);
            }
        }

        #endregion Private Methods

        //########################################################################################

    }
}