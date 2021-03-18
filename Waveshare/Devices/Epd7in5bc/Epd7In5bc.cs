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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using Waveshare.Common;

#endregion Usings

namespace Waveshare.Devices.Epd7in5bc
{
    /// <summary>
    /// Type: Waveshare 7.5inch e-Paper (B)
    /// Color: Black, White and Red
    /// Display Resolution: 640*385
    /// </summary>
    internal sealed class Epd7In5Bc : EPaperDisplayBase
    {

        //########################################################################################

        #region Properties

        /// <summary>
        /// Pixel Width of the Display
        /// </summary>
        public override int Width { get; } = 640;

        /// <summary>
        /// Pixel Height of the Display
        /// </summary>
        public override int Height { get; } = 384;

        /// <summary>
        /// Get Status Command
        /// </summary>
        protected override byte GetStatusCommand { get; } = (byte)Epd7In5BcCommands.GetStatus;

        /// <summary>
        /// Start Data Transmission Command
        /// </summary>
        protected override byte StartDataTransmissionCommand { get; } = (byte)Epd7In5BcCommands.DataStartTransmission1;

        /// <summary>
        /// Stop Data Transmission Command
        /// </summary>
        protected override byte StopDataTransmissionCommand { get; } = (byte)Epd7In5BcCommands.DataStop;

        #endregion Properties

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Clear the Display to White
        /// </summary>
        public override void Clear()
        {
            FillColor(Color.White);
            TurnOnDisplay();
        }

        /// <summary>
        /// Clear the Display to Black
        /// </summary>
        public override void ClearBlack()
        {
            FillColor(Color.Black);
            TurnOnDisplay();
        }

        /// <summary>
        /// Power the controller on.  Do not use with SleepMode.
        /// </summary>
        public override void PowerOn()
        {
            SendCommand(Epd7In5BcCommands.PowerOn);
            WaitUntilReady();
        }

        /// <summary>
        /// Power the controler off.  Do not use with SleepMode.
        /// </summary>
        public override void PowerOff()
        {
            SendCommand(Epd7In5BcCommands.PowerOff);
            WaitUntilReady();
        }

        /// <summary>
        /// Send the Display into SleepMode
        /// </summary>
        public override void Sleep()
        {
            PowerOff();
            SendCommand(Epd7In5BcCommands.DeepSleep);
            SendData(0xA5);
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

            SendCommand(Epd7In5BcCommands.PowerSetting); // POWER_SETTING
            SendData(0x37);
            SendData(0x00);

            SendCommand(Epd7In5BcCommands.PanelSetting);
            SendData(0xCF);
            SendData(0x08);

            SendCommand(Epd7In5BcCommands.PllControl);
            SendData(0x3A); // PLL:  0-15:0x3C, 15+:0x3A

            SendCommand(Epd7In5BcCommands.VcmDcSetting);
            SendData(0x28); //all temperature  range

            SendCommand(Epd7In5BcCommands.BoosterSoftStart);
            SendData(0xc7);
            SendData(0xcc);
            SendData(0x15);

            SendCommand(Epd7In5BcCommands.VcomAndDataIntervalSetting);
            SendData(0x77);

            SendCommand(Epd7In5BcCommands.TconSetting);
            SendData(0x22);

            SendCommand(Epd7In5BcCommands.SpiFlashControl);
            SendData(0x00);

            SendCommand(Epd7In5BcCommands.TconResolution);
            SendData((byte)(Width >> 8)); // source 640
            SendData((byte)(Width & 0xff));
            SendData((byte)(Height >> 8)); // gate 384
            SendData((byte)(Height & 0xff));

            SendCommand(Epd7In5BcCommands.FlashMode);
            SendData(0x03);
        }

        /// <summary>
        /// Turn the Display PowerOn after a Sleep
        /// </summary>
        protected override void TurnOnDisplay()
        {
            SendCommand(Epd7In5BcCommands.PowerOn);
            WaitUntilReady();
            SendCommand(Epd7In5BcCommands.DisplayRefresh);
            Thread.Sleep(100);
            WaitUntilReady();
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
            const byte black = 0x00;
            const byte gray = 0x02;
            const byte white = 0x03;
            const byte red = 0x04;

            if (IsMonochrom(r, g, b))
            {
                if (r <= 85)
                {
                    return black;
                }

                if (r <= 170)
                {
                    return gray;
                }

                return white;
            }

            return r >= 64 ? red : black;
        }

        #endregion Protected Methods

        //########################################################################################

        #region Internal Methods

        /// <summary>
        /// Convert a Bitmap to a Byte Array
        /// </summary>
        /// <param name="image">Bitmap image to convert</param>
        /// <returns>Byte array of image</returns>
        internal override void BitmapToData(Bitmap image)
        {
            const int pixelPerByte = 2;
            byte white = MergePixelDataInByte(0x03, 0x03);
            int maxX = Math.Min(Width, image.Width);
            int maxY = Math.Min(Height, image.Height);
            int outputStride = Width / pixelPerByte;
            int clearStart = (int)Math.Ceiling((double)(maxX / pixelPerByte)) + 1;
            byte[] output = new byte[outputStride];

            // Convert to greyscale
            BitmapData inputData = image.LockBits(new Rectangle(0, 0, maxX, maxY), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                IntPtr scanLine = inputData.Scan0;
                byte[] line = new byte[inputData.Stride];
                byte pixel1, pixel2;
                int xpos;
                for (int y = 0; y < maxY; y++, scanLine += inputData.Stride)
                {
                    Marshal.Copy(scanLine, line, 0, line.Length);
                    // Clear end of line if image is smaller than screen
                    for (int x = clearStart; x < outputStride; x++)
                    {
                        output[x] = white;
                    }
                    for (int x = 0; x < maxX; x += pixelPerByte)
                    {
                        xpos = x * 3;
                        pixel1 = ColorToByte(line[xpos + 2], line[xpos + 1], line[xpos + 0]);
                        pixel2 = xpos + 5 <= maxX ? ColorToByte(line[xpos + 5], line[xpos + 4], line[xpos + 3]) : white;
                        output[x / 2] = MergePixelDataInByte(pixel1, pixel2);
                    }
                    SendData(output);
                }
                if (maxY < Height)
                {
                    Array.Clear(output, 0, outputStride);
                    for (int y = maxY; y < Height; y++)
                    {
                        SendData(output);
                    }
                }
            }
            finally
            {
                image.UnlockBits(inputData);
            }
        }

        /// <summary>
        /// Merge two DataBytes into one Byte
        /// </summary>
        /// <param name="pixel1"></param>
        /// <param name="pixel2"></param>
        /// <returns></returns>
        internal static byte MergePixelDataInByte(byte pixel1, byte pixel2)
        {
            var output = (byte)(pixel1 << 4);
            output |= pixel2;
            return output;
        }

        #endregion Internal Methods

        //########################################################################################

        #region Private Methods

        /// <summary>
        /// Helper to send a Command based o the Epd7In5BcCommands Enum
        /// </summary>
        /// <param name="command">Command to send</param>
        private void SendCommand(Epd7In5BcCommands command)
        {
            SendCommand((byte)command);
        }

        /// <summary>
        /// Fill the screen with a color
        /// </summary>
        /// <param name="color">Color to fill the screen</param>
        private void FillColor(Color color)
        {
            const int pixelPerByte = 2;
            var pixelData = ColorToByte(color.R, color.G, color.B);
            var twoColorPixel = MergePixelDataInByte(pixelData, pixelData);
            var outputWidth = Width / pixelPerByte;
            var output = new byte[outputWidth];

            for (var x = 0; x < outputWidth; x++)
            {
                output[x] = twoColorPixel;
            }

            SendCommand(StartDataTransmissionCommand);
            for (var y = 0; y < Height; y++)
            {
                SendData(output);
            }
            SendCommand(StopDataTransmissionCommand);
        }
        #endregion Private Methods

        //########################################################################################

    }
}