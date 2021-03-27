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

using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Drawing;
using System.Linq;
using Waveshare.Devices.Epd7in5bc;
using Waveshare.Interfaces;

#endregion Usings

namespace Waveshare.Test.Devices.Epd7in5bc
{
    public class Epd7In5BcTests
    {
        private List<byte> m_DataBuffer;
        private Mock<IEPaperDisplayHardware> m_EPaperDisplayHardwareMock;

        [SetUp]
        public void Setup()
        {
            m_DataBuffer = new List<byte>();

            m_EPaperDisplayHardwareMock = new Mock<IEPaperDisplayHardware>();
            m_EPaperDisplayHardwareMock.Setup(e => e.BusyPin).Returns(PinValue.High);
            m_EPaperDisplayHardwareMock.Setup(e => e.Write(It.IsAny<byte[]>())).Callback((byte[] b) => m_DataBuffer.AddRange(b));
            m_EPaperDisplayHardwareMock.Setup(e => e.WriteByte(It.IsAny<byte>())).Callback((byte b) => m_DataBuffer.Add(b));
        }

        [Test]
        public void ConstructorTest()
        {
            using var result = new Epd7In5Bc();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);
        }

        [Test]
        public void DisposeNoHardwareTest()
        {
            using var result = new Epd7In5Bc();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);
        }

        [Test]
        public void DoubleDisposeTest()
        {
            var result = new Epd7In5Bc();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);
            result.Dispose();
            result.Dispose();
        }

        [Test]
        public void FinalizerTest()
        {
            var result = new Epd7In5Bc();
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
            var result = new Epd7In5Bc();

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
            using var result = new Epd7In5Bc();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.PowerOn();

            var validBuffer = new List<byte>
            {
                (byte)Epd7In5BcCommands.PowerOn,
                (byte)Epd7In5BcCommands.GetStatus
            };
            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }


        [Test]
        public void PowerOffTest()
        {
            using var result = new Epd7In5Bc();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.PowerOff();

            var validBuffer = new List<byte>
            {
                (byte)Epd7In5BcCommands.PowerOff,
                (byte)Epd7In5BcCommands.GetStatus
            };
            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void SleepTest()
        {
            using var result = new Epd7In5Bc();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.Sleep();

            var validBuffer = new List<byte>
            {
                (byte)Epd7In5BcCommands.PowerOff,
                (byte)Epd7In5BcCommands.GetStatus,
                (byte)Epd7In5BcCommands.DeepSleep,
                0xA5
            };
            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void ClearTest()
        {
            using var result = new Epd7In5Bc();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.Clear();

            const int pixelPerByte = 2;
            var displayBytes = result.Width / pixelPerByte * result.Height;

            const byte white = 0x03;
            var twoWhitePixel = Epd7In5Bc.MergePixelDataInByte(white, white);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5BcCommands.DataStartTransmission1
            };

            for (int i = 0; i < displayBytes; i++)
            {
                validBuffer.Add(twoWhitePixel);
            }
            validBuffer.Add((byte)Epd7In5BcCommands.DataStop);
            validBuffer.Add((byte)Epd7In5BcCommands.PowerOn);
            validBuffer.Add((byte)Epd7In5BcCommands.GetStatus);
            validBuffer.Add((byte)Epd7In5BcCommands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5BcCommands.GetStatus);

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void ClearBlackTest()
        {
            using var result = new Epd7In5Bc();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.ClearBlack();

            const int pixelPerByte = 2;
            var displayBytes = result.Width / pixelPerByte * result.Height;

            const byte black = 0x00;
            var twoBlackPixel = Epd7In5Bc.MergePixelDataInByte(black, black);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5BcCommands.DataStartTransmission1
            };

            for (int i = 0; i < displayBytes; i++)
            {
                validBuffer.Add(twoBlackPixel);
            }
            validBuffer.Add((byte)Epd7In5BcCommands.DataStop);
            validBuffer.Add((byte)Epd7In5BcCommands.PowerOn);
            validBuffer.Add((byte)Epd7In5BcCommands.GetStatus);
            validBuffer.Add((byte)Epd7In5BcCommands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5BcCommands.GetStatus);

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void DisplayImageTest()
        {
            using var result = new Epd7In5Bc();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            var image = CreateSampleBitmap(result.Width, result.Height);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5BcCommands.DataStartTransmission1
            };

            m_DataBuffer.Clear();
            result.BitmapToData(image);
            validBuffer.AddRange(m_DataBuffer);
            validBuffer.Add((byte)Epd7In5BcCommands.DataStop);
            validBuffer.Add((byte)Epd7In5BcCommands.PowerOn);
            validBuffer.Add((byte)Epd7In5BcCommands.GetStatus);
            validBuffer.Add((byte)Epd7In5BcCommands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5BcCommands.GetStatus);

            m_DataBuffer.Clear();

            result.DisplayImage(image);

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void DisplayImageSmallTest()
        {
            using var result = new Epd7In5Bc();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            var image = CreateSampleBitmap(result.Width / 2, result.Height / 2);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5BcCommands.DataStartTransmission1
            };

            m_DataBuffer.Clear();
            result.BitmapToData(image);
            validBuffer.AddRange(m_DataBuffer);
            validBuffer.Add((byte)Epd7In5BcCommands.DataStop);
            validBuffer.Add((byte)Epd7In5BcCommands.PowerOn);
            validBuffer.Add((byte)Epd7In5BcCommands.GetStatus);
            validBuffer.Add((byte)Epd7In5BcCommands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5BcCommands.GetStatus);

            m_DataBuffer.Clear();

            result.DisplayImage(image);

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void WakeUpTest()
        {
            using var result = new Epd7In5Bc();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.WakeUp();

            var validBuffer = new List<byte>
            {
                (byte)Epd7In5BcCommands.PowerSetting,
                0x37,
                0x00,
                (byte)Epd7In5BcCommands.PanelSetting,
                0xCF,
                0x08,
                (byte)Epd7In5BcCommands.PllControl,
                0x3A,
                (byte)Epd7In5BcCommands.VcmDcSetting,
                0x28,
                (byte)Epd7In5BcCommands.BoosterSoftStart,
                0xc7,
                0xcc,
                0x15,
                (byte)Epd7In5BcCommands.VcomAndDataIntervalSetting,
                0x77,
                (byte)Epd7In5BcCommands.TconSetting,
                0x22,
                (byte)Epd7In5BcCommands.SpiFlashControl,
                0x00,
                (byte)Epd7In5BcCommands.TconResolution,
                (byte)(result.Width >> 8), // source 640
                (byte)(result.Width & 0xff),
                (byte)(result.Height >> 8), // gate 384
                (byte)(result.Height & 0xff),
                (byte)Epd7In5BcCommands.FlashMode,
                0x03
            };

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        private static Bitmap CreateSampleBitmap(int width, int height)
        {
            var image = new Bitmap(width, height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var color = Color.White;

                    if (x % 2 == 0)
                    {
                        color = Color.Black;
                    }

                    if (x % 3 == 0)
                    {
                        color = Color.Red;
                    }

                    if (x % 4 == 0)
                    {
                        color = Color.Gray;
                    }

                    if (x % 5 == 0)
                    {
                        color = Color.FromArgb(255, 50, 0, 0);
                    }

                    image.SetPixel(x, y, color);
                }
            }

            return image;
        }
    }
}
