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
using System.Device.Spi;
using Waveshare.Common;

#endregion Usings

namespace Waveshare.Test.Common
{
    public class EPaperDisplayHardwareTests
    {

        private static byte s_DataByte;

        [Test]
        public void DisposeTest()
        {
            using var result = CreateEPaperDisplayHardware();

            Assert.NotNull(result, "Object should not be null");
        }

        [Test]
        public void ResetPinTest()
        {
            Assert.Inconclusive("GpiController can not be Mocked, currently not testable :-(");
        }

        [Test]
        public void BusyPinTest()
        {
            Assert.Inconclusive("GpiController can not be Mocked, currently not testable :-(");
        }

        [Test]
        public void SpiCsPinTest()
        {
            Assert.Inconclusive("GpiController can not be Mocked, currently not testable :-(");
        }

        [Test]
        public void SpiDcPinTest()
        {
            Assert.Inconclusive("GpiController can not be Mocked, currently not testable :-(");
        }

        [Test]
        public void WriteDataTest()
        {
            using var result = CreateEPaperDisplayHardware();

            for (byte b = 0; b < byte.MaxValue; b++)
            {
                result.WriteByte(b);
                Assert.AreEqual(b, s_DataByte, $"WriteByte failed with {b}");
            }
        }

        private static EPaperDisplayHardware CreateEPaperDisplayHardware()
        {
            var spiMock = new Mock<SpiDevice>();
            spiMock.Setup(s => s.WriteByte(It.IsAny<byte>())).Callback((byte b) => s_DataByte = b);

            //GpiController can not be Mocked, currently not testable :-(

            var result = new EPaperDisplayHardware(spiMock.Object, null);
            return result;
        }
    }
}
