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
using Waveshare.Devices;
using Waveshare.Interfaces;

#endregion Usings

namespace Waveshare.Test
{
    public class EPaperDisplayTests
    {
        [Test]
        public void CreateNoneTest()
        {
            using var result = EPaperDisplay.Create(EPaperDisplayType.None);
            Assert.IsNull(result, $"Enum Value {EPaperDisplayType.None} should not return a object");
        }

        [Test]
        public void CreateWaveShare7In5BcTest()
        {
            var ePaperDisplayHardwareMock = new Mock<IEPaperDisplayHardware>();
            EPaperDisplay.EPaperDisplayHardware = new Lazy<IEPaperDisplayHardware>(() => ePaperDisplayHardwareMock.Object);

            using var result = EPaperDisplay.Create(EPaperDisplayType.WaveShare7In5Bc);
            Assert.NotNull(result, $"Enum Value {EPaperDisplayType.WaveShare7In5Bc} should return a object");
        }

        [Test]
        public void GetEPaperDisplayHardwareTest()
        {
            var lazyHardware = EPaperDisplay.EPaperDisplayHardware;
            Assert.NotNull(lazyHardware);

            using var result = lazyHardware.Value;
            Assert.NotNull(result, "EPaperDisplayHardware should not return null");

            var ePaperDisplayHardwareMock = new Mock<IEPaperDisplayHardware>();
            EPaperDisplay.EPaperDisplayHardware = new Lazy<IEPaperDisplayHardware>(() => ePaperDisplayHardwareMock.Object);

            using var result2 = EPaperDisplay.EPaperDisplayHardware.Value;
            Assert.NotNull(result2, "EPaperDisplayHardware should not return null");
        }
    }
}