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

using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Waveshare.Devices.Epd7in5_V2;
using Waveshare.Image.Bitmap;
using Waveshare.Interfaces;

#endregion Usings

namespace Waveshare.Test.Devices.Epd7in5_V2
{
    // ReSharper disable once InconsistentNaming
    public class Epd7In5_V2Tests
    {

        //########################################################################################

        #region Fields

        private List<byte> m_DataBuffer;
        private Mock<IEPaperDisplayHardware> m_EPaperDisplayHardwareMock;

        #endregion Fields

        //########################################################################################

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_DataBuffer = new List<byte>();

            m_EPaperDisplayHardwareMock = new Mock<IEPaperDisplayHardware>();
            m_EPaperDisplayHardwareMock.Setup(e => e.BusyPin).Returns(PinValue.High);
            m_EPaperDisplayHardwareMock.Setup(e => e.Write(It.IsAny<byte[]>())).Callback((byte[] b) => m_DataBuffer.AddRange(b));
            m_EPaperDisplayHardwareMock.Setup(e => e.WriteByte(It.IsAny<byte>())).Callback((byte b) => m_DataBuffer.Add(b));
        }

        #endregion Setup

        //########################################################################################

        #region Tests

        [Test]
        public void ConstructorTest()
        {
            using var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);
        }

        [Test]
        public void DisposeNoHardwareTest()
        {
            using var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);
        }

        [Test]
        public void DoubleDisposeTest()
        {
            var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);
            result.Dispose();
            result.Dispose();
        }

        [Test]
        public void FinalizerTest()
        {
            var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            Assert.NotNull(result, "Object should not be null");

            // ReSharper disable once RedundantAssignment
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            result = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [Test]
        public void FinalizerNoHardwareTest()
        {
            var result = new Epd7In5_V2();

            Assert.NotNull(result, "Object should not be null");

            // ReSharper disable once RedundantAssignment
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            result = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [Test]
        public void PowerOnTest()
        {
            using var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.PowerOn();

            var validBuffer = new List<byte>
            {
                (byte)Epd7In5_V2Commands.PowerOn,
                (byte)Epd7In5_V2Commands.GetStatus
            };
            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }


        [Test]
        public void PowerOffTest()
        {
            using var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.PowerOff();

            var validBuffer = new List<byte>
            {
                (byte)Epd7In5_V2Commands.PowerOff,
                (byte)Epd7In5_V2Commands.GetStatus
            };
            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void SleepTest()
        {
            using var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.Sleep();

            var validBuffer = new List<byte>
            {
                (byte)Epd7In5_V2Commands.PowerOff,
                (byte)Epd7In5_V2Commands.GetStatus,
                (byte)Epd7In5_V2Commands.DeepSleep,
                0xA5
            };
            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void ClearTest()
        {
            using var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.Clear();

            const int pixelPerByte = 8;
            var displayBytes = result.Width / pixelPerByte * result.Height;

            const byte white = 0x00;
            var eightWhitePixel = result.MergePixelDataInByte(white, white, white, white, white, white, white, white);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5_V2Commands.DataStartTransmission1
            };

            for (int i = 0; i < displayBytes; i++)
            {
                validBuffer.Add(eightWhitePixel);
            }

            validBuffer.Add((byte)Epd7In5_V2Commands.DataStartTransmission2);

            for (int i = 0; i < displayBytes; i++)
            {
                validBuffer.Add(eightWhitePixel);
            }
            validBuffer.Add((byte)Epd7In5_V2Commands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5_V2Commands.GetStatus);

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }


        [Test]
        public void ClearBlackTest()
        {
            using var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.ClearBlack();

            const int pixelPerByte = 8;
            var displayBytes = result.Width / pixelPerByte * result.Height;

            const byte black = 0x01;
            var eightBlackPixel = result.MergePixelDataInByte(black, black, black, black, black, black, black, black);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5_V2Commands.DataStartTransmission2
            };

            for (int i = 0; i < displayBytes; i++)
            {
                validBuffer.Add(eightBlackPixel);
            }

            validBuffer.Add((byte)Epd7In5_V2Commands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5_V2Commands.GetStatus);

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void DisplayImageTest()
        {
            using var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            var image = CommonTestData.CreateSampleBitmap(result.Width, result.Height);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5_V2Commands.DataStartTransmission2
            };

            m_DataBuffer.Clear();

            validBuffer.AddRange(SendBitmapToDevice(image, result.Width, result.Height));
            validBuffer.Add((byte)Epd7In5_V2Commands.DataStop);
            validBuffer.Add((byte)Epd7In5_V2Commands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5_V2Commands.GetStatus);

            m_DataBuffer.Clear();

            var bitmapEPaperDisplay = new BitmapLoader(result);
            bitmapEPaperDisplay.DisplayImage(image);

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void DisplayImageSmallTest()
        {
            using var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            var image = CommonTestData.CreateSampleBitmap(result.Width / 2, result.Height / 2);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5_V2Commands.DataStartTransmission2
            };

            m_DataBuffer.Clear();

            validBuffer.AddRange(SendBitmapToDevice(image, result.Width, result.Height));
            validBuffer.Add((byte)Epd7In5_V2Commands.DataStop);
            validBuffer.Add((byte)Epd7In5_V2Commands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5_V2Commands.GetStatus);

            m_DataBuffer.Clear();

            var bitmapEPaperDisplay = new BitmapLoader(result);
            bitmapEPaperDisplay.DisplayImage(image);

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void WakeUpTest()
        {
            using var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.WakeUp();

            var validBuffer = new List<byte>
            {
                (byte)Epd7In5_V2Commands.BoosterSoftStart,
                0x17,
                0x17,
                0x27,
                0x17,
                (byte)Epd7In5_V2Commands.PowerSetting,
                0x07,
                0x17,
                0x3f,
                0x3f,
                (byte)Epd7In5_V2Commands.PowerOn,
                (byte)Epd7In5_V2Commands.GetStatus,
                (byte)Epd7In5_V2Commands.PanelSetting,
                0x1F,
                (byte)Epd7In5_V2Commands.TconResolution,
                0x03,
                0x20,
                0x01,
                0xe0,
                (byte)Epd7In5_V2Commands.DualSpi,
                0x00,
                (byte)Epd7In5_V2Commands.VcomAndDataIntervalSetting,
                0x10,
                0x07,
                (byte)Epd7In5_V2Commands.TconSetting,
                0x22
            };

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void TestMergePixelDataInByte()
        {
            using var result = new Epd7In5_V2();

            var random = new Random();
            for (int i = 0; i < 200; i++)
            {
                var b1 = (byte)random.Next(0, 0x0F);
                var b2 = (byte)random.Next(0, 0x0F);
                var b3 = (byte)random.Next(0, 0x0F);
                var b4 = (byte)random.Next(0, 0x0F);
                var b5 = (byte)random.Next(0, 0x0F);
                var b6 = (byte)random.Next(0, 0x0F);
                var b7 = (byte)random.Next(0, 0x0F);
                var b8 = (byte)random.Next(0, 0x0F);

                var oldResult = MergePixelDataInByte(b1, b2, b3, b4, b5, b6, b7, b8);
                var newResult = result.MergePixelDataInByte(b1, b2, b3, b4, b5, b6, b7, b8);

                Assert.AreEqual(oldResult, newResult, $"Merged Byte Run {i} is wrong. Expected {oldResult}, Returned {newResult}");
            }
        }

#endregion Tests

        //########################################################################################

        #region Old working Implementation

        /// <summary>
        /// Original Method: Merge eight DataBytes into one Byte
        /// </summary>
        /// <param name="pixel1"></param>
        /// <param name="pixel2"></param>
        /// <param name="pixel3"></param>
        /// <param name="pixel4"></param>
        /// <param name="pixel5"></param>
        /// <param name="pixel6"></param>
        /// <param name="pixel7"></param>
        /// <param name="pixel8"></param>
        /// <returns></returns>
        private static byte MergePixelDataInByte(byte pixel1, byte pixel2, byte pixel3, byte pixel4, byte pixel5, byte pixel6, byte pixel7, byte pixel8)
        {
            var output = (byte)((pixel1 << 7) | (pixel2 << 6) | (pixel3 << 5) | (pixel4 << 4) | (pixel5 << 3) | (pixel6 << 2) | (pixel7 << 1) | pixel8);
            return output;
        }

        /// <summary>
        /// Send a Bitmap as Byte Array to the Device
        /// </summary>
        /// <param name="image">Bitmap image to convert</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        internal static byte[] SendBitmapToDevice(Bitmap image, int width, int height)
        {
            var outputArray = new List<Byte>();

            const int pixelPerByte = 8;
            int maxX = Math.Min(width, image.Width);
            int maxY = Math.Min(height, image.Height);

            var inputData = image.LockBits(new Rectangle(0, 0, maxX, maxY), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                var colorBytesPerPixel = inputData.Stride / inputData.Width;
                var deviceLineWithInByte = inputData.Width * colorBytesPerPixel;
                var deviceStep = colorBytesPerPixel * pixelPerByte;

                IntPtr scanLine = inputData.Scan0;
                byte[] line = new byte[inputData.Stride];

                for (var y = 0; y < height; y++)
                {
                    var outputLine = CloneWhiteScanLine(width/pixelPerByte);

                    if (y < maxY)
                    {
                        Marshal.Copy(scanLine, line, 0, line.Length);

                        for (var x = 0; x < deviceLineWithInByte; x += deviceStep)
                        {
                            outputLine[x / deviceStep] = GetDevicePixels(x, line);
                        }

                        scanLine += inputData.Stride;
                    }

                    outputArray.AddRange(outputLine);
                }
            }
            finally
            {
                image.UnlockBits(inputData);
            }

            return outputArray.ToArray();
        }

        /// <summary>
        /// Get the byte on the device for the selected pixels
        /// </summary>
        /// <param name="xPosition"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private static byte GetDevicePixels(int xPosition, byte[] line)
        {
            var pixels = new byte[8];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = GetPixelFromArray(line, xPosition, i);
            }

            return MergePixelDataInByte(pixels);
        }

        /// <summary>
        /// Get a empty line on the device
        /// </summary>
        /// <param name="inputDataStride"></param>
        /// <returns></returns>
        private static byte[] CloneWhiteScanLine(int inputDataStride)
        {
            return new byte[inputDataStride];
        }

        /// <summary>
        /// Get Pixel from the byte array
        /// </summary>
        /// <param name="line"></param>
        /// <param name="xPosition"></param>
        /// <param name="pixel"></param>
        /// <returns></returns>
        private static byte GetPixelFromArray(byte[] line, int xPosition, int pixel)
        {
            var pixelWith = 3 * pixel;

            var colorR = xPosition + pixelWith + 2;
            var colorG = xPosition + pixelWith + 1;
            var colorB = xPosition + pixelWith + 0;

            if (colorR >= line.Length)
            {
                return 0;
            }

            return ColorToByte(line[colorR], line[colorG], line[colorB]);
        }

        /// <summary>
        /// Merge eight DataBytes into one Byte
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns></returns>
        internal static byte MergePixelDataInByte(byte[] pixel)
        {
            const int bitsInByte = 8;
            var bitMoveLength = bitsInByte / 8;

            byte output = 0;

            for (var i = 0; i < pixel.Length; i++)
            {
                var moveFactor = bitsInByte - bitMoveLength * (i + 1);
                output |= (byte)(pixel[i] << moveFactor);
            }

            return output;
        }

        /// <summary>
        /// Convert a pixel to a DataByte
        /// </summary>
        /// <param name="r">Red color byte</param>
        /// <param name="g">Green color byte</param>
        /// <param name="b">Blue color byte</param>
        /// <returns>Pixel converted to specific byte value for the hardware</returns>
        private static byte ColorToByte(byte r, byte g, byte b)
        {
            if (r == g && r == b)
            {
                return (byte)(byte.MaxValue - r);
            }

            return (byte)(byte.MaxValue - (r * 0.299 + g * 0.587 + b * 0.114));
        }

        #endregion Old working Implementation

        //########################################################################################

    }
}
