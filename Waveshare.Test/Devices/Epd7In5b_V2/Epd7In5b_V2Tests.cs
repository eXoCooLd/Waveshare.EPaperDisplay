#region Usings

using Moq;
using NUnit.Framework;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Waveshare.Devices.Epd7in5b_V2;
using Waveshare.Image;
using Waveshare.Interfaces.Internal;

#endregion Usings

namespace Waveshare.Test.Devices.Epd7in5b_V2
{
    // ReSharper disable once InconsistentNaming
    public class Epd7In5b_V2Tests
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
            m_EPaperDisplayHardwareMock.Setup(e => e.Write(It.IsAny<MemoryStream>())).Callback((MemoryStream b) => m_DataBuffer.AddRange(b.ToArray()));
        }

        [Test]
        public void ConstructorTest()
        {
            using var result = new Epd7In5b_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);
        }

        [Test]
        public void DisposeNoHardwareTest()
        {
            using var result = new Epd7In5b_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);
        }

        [Test]
        public void DoubleDisposeTest()
        {
            var result = new Epd7In5b_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);
            result.Dispose();
            result.Dispose();
        }

        [Test]
        public void FinalizerTest()
        {
            var result = new Epd7In5b_V2();
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
            var result = new Epd7In5b_V2();

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
            using var result = new Epd7In5b_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.PowerOn();

            var validBuffer = new List<byte>
            {
                (byte)Epd7In5b_V2Commands.PowerOn,
                (byte)Epd7In5b_V2Commands.GetStatus
            };
            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }


        [Test]
        public void PowerOffTest()
        {
            using var result = new Epd7In5b_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.PowerOff();

            var validBuffer = new List<byte>
            {
                (byte)Epd7In5b_V2Commands.PowerOff,
                (byte)Epd7In5b_V2Commands.GetStatus
            };
            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void SleepTest()
        {
            using var result = new Epd7In5b_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.Sleep();

            var validBuffer = new List<byte>
            {
                (byte)Epd7In5b_V2Commands.PowerOff,
                (byte)Epd7In5b_V2Commands.GetStatus,
                (byte)Epd7In5b_V2Commands.DeepSleep,
                0xA5
            };
            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void ClearTest()
        {
            using var result = new Epd7In5b_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.Clear();

            const int pixelPerByte = 8;
            var displayBytes = result.Width / pixelPerByte * result.Height;

            const byte white = 0x01;
            const byte black = 0x00;
            var eightWhitePixel = result.MergePixelDataInByte(white, white, white, white, white, white, white, white);
            var eightBlackPixel = result.MergePixelDataInByte(black, black, black, black, black, black, black, black);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5b_V2Commands.DataStartTransmission1
            };

            for (int i = 0; i < displayBytes; i++)
            {
                validBuffer.Add(eightWhitePixel);
            }

            validBuffer.Add((byte)Epd7In5b_V2Commands.DataStartTransmission2);

            for (int i = 0; i < displayBytes; i++)
            {
                validBuffer.Add(eightBlackPixel);
            }

            validBuffer.Add((byte)Epd7In5b_V2Commands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5b_V2Commands.GetStatus);

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }


        [Test]
        public void ClearBlackTest()
        {
            using var result = new Epd7In5b_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.ClearBlack();

            const int pixelPerByte = 8;
            var displayBytes = result.Width / pixelPerByte * result.Height;

            const byte black = 0x00;
            var eightBlackPixel = result.MergePixelDataInByte(black, black, black, black, black, black, black, black);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5b_V2Commands.DataStartTransmission1
            };

            for (int i = 0; i < displayBytes; i++)
            {
                validBuffer.Add(eightBlackPixel);
            }

            validBuffer.Add((byte)Epd7In5b_V2Commands.DataStartTransmission2);

            for (int i = 0; i < displayBytes; i++)
            {
                validBuffer.Add(eightBlackPixel);
            }

            validBuffer.Add((byte)Epd7In5b_V2Commands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5b_V2Commands.GetStatus);

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void DisplayImageTest()
        {
            using var result = new Epd7In5b_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            var image = CommonTestData.CreateSampleBitmap(result.Width, result.Height);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5b_V2Commands.DataStartTransmission1
            };

            SeparateColors(image, out var imageBw, out var imageRed);

            using (imageBw)
            {
                validBuffer.AddRange(SendBitmapToDevice(imageBw, result.Width, result.Height, byte.MaxValue));
            }

            validBuffer.Add((byte)Epd7In5b_V2Commands.DataStartTransmission2);

            using (imageRed)
            {
                validBuffer.AddRange(SendBitmapToDevice(imageRed, result.Width, result.Height, 0));
            }

            validBuffer.Add((byte)Epd7In5b_V2Commands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5b_V2Commands.GetStatus);

            m_DataBuffer.Clear();

            var bitmapEPaperDisplay = new SKBitmapLoader(result);
            bitmapEPaperDisplay.DisplayImage(image);

            Assert.AreEqual(validBuffer.Count, m_DataBuffer.Count, "Data Length is wrong");
            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void DisplayImageSmallTest()
        {
            using var result = new Epd7In5b_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            var image = CommonTestData.CreateSampleBitmap(result.Width / 2, result.Height / 2);

            var validBuffer = new List<byte>
            {
                (byte) Epd7In5b_V2Commands.DataStartTransmission1
            };

            SeparateColors(image, out var imageBw, out var imageRed);

            using (imageBw)
            {
                validBuffer.AddRange(SendBitmapToDevice(imageBw, result.Width, result.Height, byte.MaxValue));
            }

            validBuffer.Add((byte)Epd7In5b_V2Commands.DataStartTransmission2);

            using (imageRed)
            {
                validBuffer.AddRange(SendBitmapToDevice(imageRed, result.Width, result.Height, 0));
            }

            validBuffer.Add((byte)Epd7In5b_V2Commands.DisplayRefresh);
            validBuffer.Add((byte)Epd7In5b_V2Commands.GetStatus);

            m_DataBuffer.Clear();

            var bitmapEPaperDisplay = new SKBitmapLoader(result);
            bitmapEPaperDisplay.DisplayImage(image);

            for (int i = 0; i < m_DataBuffer.Count; i++)
            {
                if (i < validBuffer.Count)
                {
                    if (m_DataBuffer[i] != validBuffer[i])
                    {
                        Debug.WriteLine($"{i} - {m_DataBuffer[i]} - {validBuffer[i]}");
                    }
                }
                else
                {
                    Debug.WriteLine($"{i} - {m_DataBuffer[i]} - X");
                }
            }

            Assert.AreEqual(validBuffer.Count, m_DataBuffer.Count, "Data Length is wrong");
            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void WakeUpTest()
        {
            using var result = new Epd7In5b_V2();
            result.Initialize(m_EPaperDisplayHardwareMock.Object);

            m_DataBuffer.Clear();

            result.WakeUp();

            var validBuffer = new List<byte>
            {
                (byte)Epd7In5b_V2Commands.BoosterSoftStart,
                0x17,
                0x17,
                0x27,
                0x17,
                (byte)Epd7In5b_V2Commands.PowerSetting,
                0x07,
                0x07,
                0x3f,
                0x3f,
                (byte)Epd7In5b_V2Commands.PowerOn,
                (byte)Epd7In5b_V2Commands.GetStatus,
                (byte)Epd7In5b_V2Commands.PanelSetting,
                0x0f,
                (byte)Epd7In5b_V2Commands.TconResolution,
                0x03,
                0x20,
                0x01,
                0xe0,
                (byte)Epd7In5b_V2Commands.DualSpi,
                0x00,
                (byte)Epd7In5b_V2Commands.VcomAndDataIntervalSetting,
                0x11,
                0x07,
                (byte)Epd7In5b_V2Commands.TconSetting,
                0x22,
                (byte)Epd7In5b_V2Commands.GateSourceStartSetting,
                0x00,
                0x00,
                0x00,
                0x00
        };

            Assert.IsTrue(m_DataBuffer.SequenceEqual(validBuffer), "Command Data Sequence is wrong");
        }

        [Test]
        public void TestMergePixelDataInByte()
        {
            using var result = new Epd7In5b_V2();

            var random = new Random();
            for (int i = 0; i < 200; i++)
            {
                var value = random.Next(0, byte.MaxValue);
                var b1 = (value & 128) > 0 ? byte.MaxValue : byte.MinValue;
                var b2 = (value & 64) > 0 ? byte.MaxValue : byte.MinValue;
                var b3 = (value & 32) > 0 ? byte.MaxValue : byte.MinValue;
                var b4 = (value & 16) > 0 ? byte.MaxValue : byte.MinValue;
                var b5 = (value & 8) > 0 ? byte.MaxValue : byte.MinValue;
                var b6 = (value & 4) > 0 ? byte.MaxValue : byte.MinValue;
                var b7 = (value & 2) > 0 ? byte.MaxValue : byte.MinValue;
                var b8 = (value & 1) > 0 ? byte.MaxValue : byte.MinValue;

                var newResult = result.MergePixelDataInByte(b1, b2, b3, b4, b5, b6, b7, b8);

                Assert.AreEqual(value, newResult, $"Merged Byte Run {i} is wrong. Expected {value}, Returned {newResult}");
            }
        }

        /// <summary>
        /// Separate Image into Black and Red Image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageBw"></param>
        /// <param name="imageRed"></param>
        private static void SeparateColors(SKBitmap image, out SKBitmap imageBw, out SKBitmap imageRed)
        {
            int width = image.Width;
            int height = image.Height;

            imageBw = new SKBitmap(width, height);
            imageRed = new SKBitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = image.GetPixel(x, y);

                    if (IsRed(pixel.Red, pixel.Green, pixel.Blue))
                    {
                        imageRed.SetPixel(x, y, SKColors.Red);
                        imageBw.SetPixel(x, y, SKColors.Black);
                    }
                    else
                    {
                        imageBw.SetPixel(x, y, pixel);
                        imageRed.SetPixel(x, y, SKColors.Black);
                    }
                }
            }
        }


        /// <summary>
        /// Check if a Pixel is Monochrom (white, gray or black)
        /// </summary>
        /// <param name="r">Red color byte</param>
        /// <param name="g">Green color byte</param>
        /// <param name="b">Blue color byte</param>
        /// <returns></returns>
        private static bool IsMonochrome(byte r, byte g, byte b)
        {
            return r == g && g == b;
        }

        /// <summary>
        /// Check if a Pixel is Red
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static bool IsRed(byte r, byte g, byte b)
        {
            if (IsMonochrome(r, g, b))
            {
                return false;
            }

            return r > (g + 20) && r > (b + 20) && r >= 64;
        }

        /// <summary>
        /// Send a Bitmap as Byte Array to the Device
        /// </summary>
        /// <param name="image">Bitmap image to convert</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="defaultColor"></param>
        internal static byte[] SendBitmapToDevice(SKBitmap image, int width, int height, byte defaultColor)
        {
            var outputArray = new List<byte>();

            const int pixelPerByte = 8;
            int maxX = Math.Min(width, image.Width);
            int maxY = Math.Min(height, image.Height);

            var colorBytesPerPixel = image.BytesPerPixel;
            var stride = maxX * image.BytesPerPixel;
            var deviceLineWithInByte = image.Width * colorBytesPerPixel;
            var deviceStep = colorBytesPerPixel * pixelPerByte;

            IntPtr scanLine = image.GetPixels();
            byte[] line = new byte[stride];

            for (var y = 0; y < height; y++)
            {
                var outputLine = CloneWhiteScanLine(width / pixelPerByte, defaultColor);

                if (y < maxY)
                {
                    Marshal.Copy(scanLine, line, 0, line.Length);

                    for (var x = 0; x < deviceLineWithInByte; x += deviceStep)
                    {
                        outputLine[x / deviceStep] = GetDevicePixels(x, line);
                    }

                    scanLine += stride;
                }

                outputArray.AddRange(outputLine);
            }

            return outputArray.ToArray();
        }

        /// <summary>
        /// Get a empty line on the device
        /// </summary>
        /// <param name="inputDataStride"></param>
        /// <param name="defaultColor"></param>
        /// <returns></returns>
        private static byte[] CloneWhiteScanLine(int inputDataStride, byte defaultColor)
        {
            var line = new byte[inputDataStride];
            for (var i = 0; i < line.Length; i++)
            {
                line[i] = defaultColor;
            }

            return line;
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
        /// Get Pixel from the byte array
        /// </summary>
        /// <param name="line"></param>
        /// <param name="xPosition"></param>
        /// <param name="pixel"></param>
        /// <returns></returns>
        private static byte GetPixelFromArray(byte[] line, int xPosition, int pixel)
        {
            var pixelWidth = 4 * pixel;

            var colorB = xPosition + pixelWidth;
            var colorG = xPosition + ++pixelWidth;
            var colorR = xPosition + ++pixelWidth;

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
            const int bitStates = 2;
            const int bitsInByte = 8;

            var bitMoveLength = bitsInByte / 8;
            var maxValue = (byte)Math.Pow(bitStates, bitMoveLength) - 1;

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
        /// Convert a pixel to a DataByte
        /// </summary>
        /// <param name="r">Red color byte</param>
        /// <param name="g">Green color byte</param>
        /// <param name="b">Blue color byte</param>
        /// <returns>Pixel converted to specific byte value for the hardware</returns>
        private static byte ColorToByte(byte r, byte g, byte b)
        {
            if (IsRed(r, g, b))
            {
                return 1;
            }

            return (byte)((r * 0.299 + g * 0.587 + b * 0.114 + .005) < 128 ? 0 : 1);
        }
    }
}
