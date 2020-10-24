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
using System.Linq;
using Waveshare.Devices.Epd7in5_V2;
using Waveshare.Interfaces;

#endregion Usings

namespace Waveshare.Test.Devices.Epd7in5_V2
{
    // ReSharper disable once InconsistentNaming
    public class Epd7In5_V2Tests
    {
        private List<byte> m_DataBuffer;
        private Mock<IEPaperDisplayHardware> m_EPaperDisplayHardwareMock;

        [SetUp]
        public void Setup()
        {
            m_DataBuffer = new List<byte>();

            m_EPaperDisplayHardwareMock = new Mock<IEPaperDisplayHardware>();
            m_EPaperDisplayHardwareMock.Setup(e => e.BusyPin).Returns(PinValue.High);
            m_EPaperDisplayHardwareMock.Setup(e => e.WriteByte(It.IsAny<byte>())).Callback((byte b) => m_DataBuffer.Add(b));
        }

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
            var eightWhitePixel = Epd7In5_V2.MergePixelDataInByte(white, white, white, white, white, white, white, white);

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
            var eightBlackPixel = Epd7In5_V2.MergePixelDataInByte(black, black, black, black, black, black, black, black);

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

            var image = CreateSampleBitmap(result.Width, result.Height);

            m_DataBuffer.Clear();

            result.DisplayImage(image);

            var imageData = result.BitmapToData(image);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5_V2Commands.DataStartTransmission2
            };

            validBuffer.AddRange(imageData);

            validBuffer.Add((byte)Epd7In5_V2Commands.DataStop);
            validBuffer.Add((byte)Epd7In5_V2Commands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5_V2Commands.GetStatus);

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void DisplayImageSmallTest()
        {
            using var result = new Epd7In5_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            var image = CreateSampleBitmap(result.Width / 2, result.Height / 2);

            m_DataBuffer.Clear();

            result.DisplayImage(image);

            var imageData = result.BitmapToData(image);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5_V2Commands.DataStartTransmission2
            };

            validBuffer.AddRange(imageData);

            validBuffer.Add((byte)Epd7In5_V2Commands.DataStop);
            validBuffer.Add((byte)Epd7In5_V2Commands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5_V2Commands.GetStatus);

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
