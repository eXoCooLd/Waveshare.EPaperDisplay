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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Waveshare.Common;

#endregion Usings

namespace Waveshare.Devices.Epd7in5b_V2
{
    /// <summary>
    /// Type: Waveshare 7.5inch e-Paper V2
    /// Color: Black, White and Red
    /// Display Resolution: 800*480
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal sealed class Epd7In5b_V2 : EPaperDisplayBase
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
        protected override byte GetStatusCommand { get; } = (byte)Epd7In5b_V2Commands.GetStatus;

        /// <summary>
        /// Start Data Transmission Command
        /// </summary>
        protected override byte StartDataTransmissionCommand { get; } = (byte)Epd7In5b_V2Commands.DataStartTransmission2;

        /// <summary>
        /// Stop Data Transmission Command
        /// </summary>
        protected override byte StopDataTransmissionCommand { get; } = (byte)Epd7In5b_V2Commands.DataStop;

        #endregion Properties

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Clear the Display to White
        /// </summary>
        public override void Clear()
        {
            FillColor(Epd7In5b_V2Commands.DataStartTransmission1, Color.White);
            FillColor(Epd7In5b_V2Commands.DataStartTransmission2, Color.Black);
            TurnOnDisplay();
        }

        /// <summary>
        /// Clear the Display to Red
        /// </summary>
        public void ClearRed()
        {
            FillColor(Epd7In5b_V2Commands.DataStartTransmission1, Color.White);
            FillColor(Epd7In5b_V2Commands.DataStartTransmission2, Color.White);
            TurnOnDisplay();
        }

        /// <summary>
        /// Clear the Display to Black
        /// </summary>
        public override void ClearBlack()
        {
            FillColor(Epd7In5b_V2Commands.DataStartTransmission1, Color.Black);
            FillColor(Epd7In5b_V2Commands.DataStartTransmission2, Color.Black);
            TurnOnDisplay();
        }

        /// <summary>
        /// Power the controller on.  Do not use with SleepMode.
        /// </summary>
        public override void PowerOn()
        {
            SendCommand(Epd7In5b_V2Commands.PowerOn);
            DeviceWaitUntilReady();
        }

        /// <summary>
        /// Power the controller off.  Do not use with SleepMode.
        /// </summary>
        public override void PowerOff()
        {
            SendCommand(Epd7In5b_V2Commands.PowerOff);
            DeviceWaitUntilReady();
        }

        /// <summary>
        /// Send the Display into SleepMode
        /// </summary>
        public override void Sleep()
        {
            PowerOff();
            SendCommand(Epd7In5b_V2Commands.DeepSleep);
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

        /// <summary>
        /// Display a Image on the Display
        /// </summary>
        /// <param name="image">Bitmap that should be displayed</param>
        public override void DisplayImage(Bitmap image)
        {
            SeparateColors(image, out var bw, out var red);

            SendCommand(Epd7In5b_V2Commands.DataStartTransmission1);
            SendBitmapToDevice(bw);
            SendCommand(Epd7In5b_V2Commands.DataStartTransmission2);
            SendBitmapToDevice(red);

            TurnOnDisplay();
        }

        private void SeparateColors(Bitmap image, out Bitmap image_bw, out Bitmap image_red)
        {
            int width = image.Width;
            int height = image.Height;

            image_bw = new Bitmap(width, height);
            image_red = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = image.GetPixel(x, y);

                     if(IsRed(pixel.R, pixel.G, pixel.B))
                    {
                        image_red.SetPixel(x, y, pixel);
                        image_bw.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        image_bw.SetPixel(x, y, pixel);
                        image_red.SetPixel(x, y, Color.Black);
                    }
                }
            }

            image_bw.Save("test_bw.bmp");
            image_red.Save("test_red.bmp");
        }

        private byte[,] ConvertBitmapToByteArray(Bitmap image)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();
            int maxX = Math.Min(Width, image.Width);
            int maxY = Math.Min(Height, image.Height);

            var inputData = image.LockBits(new Rectangle(0, 0, maxX, maxY), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[,] output = new byte[2, inputData.Stride * Height];

            try
            {
                IntPtr pointer = inputData.Scan0;
                int stride = inputData.Stride;
                int length = inputData.Stride * Height;
                int index = 0;
                int bytecount = 0;

                byte currentByte_white = 0x00;
                byte currentByte_red = 0x00;

                byte[] values = new byte[length];

                Marshal.Copy(pointer, values, 0, length);

                for (int row = 0; row < Height; row++)
                {
                    for (int column = 0; column < Width; column++)
                    {
                        var pixel_r = values[(column * 3) + (row * stride) + 2];
                        var pixel_g = values[(column * 3) + (row * stride) + 1];
                        var pixel_b = values[(column * 3) + (row * stride)];

                        var pixel = ColorToByte(pixel_r, pixel_g, pixel_b);

                        

                     //switch (pixel)
                     //{
                     //    case Epd7in5b_V2Colors.White:
                     //        currentByte_white |= (byte)(1 << (7 - index));
                     //        break;
                     //    case Epd7in5b_V2Colors.Red:
                     //        currentByte_red |= (byte)(1 << (7 - index));
                     //        break;
                     //}

                        if(index == 7)
                        {
                            index = 0;

                            output[0, bytecount] = currentByte_white;
                            output[1, bytecount] = currentByte_red;


                            currentByte_white = 0x00;
                            currentByte_red = 0x00;
                        }
                        else
                        {
                            index++;
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception.Message);
                return null;
            }
            finally
            {
                image.UnlockBits(inputData);
            }

            Console.WriteLine("Converting Image: " + stopWatch.Elapsed);
            return output;
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

            SendCommand(Epd7In5b_V2Commands.BoosterSoftStart);
            SendData(0x17);
            SendData(0x17);
            SendData(0x27);
            SendData(0x17);

            SendCommand(Epd7In5b_V2Commands.PowerSetting);
            SendData(0x07); // VGH: 20V
            SendData(0x07); // VGL: -20V
            SendData(0x3f); // VDH: 15V
            SendData(0x3f); // VDL: -15V

            SendCommand(Epd7In5b_V2Commands.PowerOn);
            Thread.Sleep(100);
            DeviceWaitUntilReady();

            SendCommand(Epd7In5b_V2Commands.PanelSetting);
            SendData(0x0F); // KW-3f   KWR-2F	BWROTP 0f	BWOTP 1f

            SendCommand(Epd7In5b_V2Commands.TconResolution);
            SendData(0x03); // source 800
            SendData(0x20);
            SendData(0x01); // gate 480
            SendData(0xe0);

            SendCommand(Epd7In5b_V2Commands.DualSpi);
            SendData(0x00);

            SendCommand(Epd7In5b_V2Commands.VcomAndDataIntervalSetting);
            SendData(0x11);
            SendData(0x07);

            SendCommand(Epd7In5b_V2Commands.TconSetting);
            SendData(0x22);

            SendCommand(Epd7In5b_V2Commands.GateSourceStartSetting);  // Resolution setting
            SendData(0x00);
            SendData(0x00);//800*480
            SendData(0x00);
            SendData(0x00);
        }

        /// <summary>
        /// Turn the Display PowerOn after a Sleep
        /// </summary>
        protected override void TurnOnDisplay()
        {
            SendCommand(Epd7In5b_V2Commands.DisplayRefresh);
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
            if (IsMonochrom(r, g, b) || IsRed(r, g, b))
            {
                return (byte)(byte.MinValue + r);
            }

            return (byte)(byte.MinValue + (r * 0.299 + g * 0.587 + b * 0.114));
        }
        #endregion Protected Methods

        //########################################################################################

        #region Internal Methods

        /// <summary>
        /// Send a Bitmap as Byte Array to the Device
        /// </summary>
        /// <param name="image">Bitmap image to convert</param>
        internal override void SendBitmapToDevice(Bitmap image)
        {
            const int pixelPerByte = 8;
            int maxX = Math.Min(Width, image.Width);
            int maxY = Math.Min(Height, image.Height);
            sbyte[,] data = new sbyte[maxX, 2];
            byte[] masks = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
            int outputStride = Width / pixelPerByte;
            byte[] output = new byte[outputStride];
            int lineC = 0;

            var inputData = image.LockBits(new Rectangle(0, 0, maxX, maxY), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                IntPtr scanLine = inputData.Scan0;
                byte[] inputLine = new byte[inputData.Stride];
                bool odd = false;
                bool dither = false;
                sbyte error;
                bool j;

                for (int y = 0; y < maxY; y++, scanLine += inputData.Stride)
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
                        var xpos = x * 3;
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
            finally
            {
                image.UnlockBits(inputData);
            }
        }

        #endregion

        //########################################################################################

        #region Private Methods

        /// <summary>
        /// Helper to send a Command based o the Epd7In5b_V2Commands Enum
        /// </summary>
        /// <param name="command">Command to send</param>
        private void SendCommand(Epd7In5b_V2Commands command)
        {
            SendCommand((byte)command);
        }

        /// <summary>
        /// Fill the screen with a color
        /// </summary>
        /// <param name="command">Start Data Transmission Command</param>
        /// <param name="color">Color to fill the screen</param>
        private void FillColor(Epd7In5b_V2Commands command, Color color)
        {
            var outputLine = GetColoredLineOnDevice(color);

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