#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2021 Greg Cannon

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

using System.IO;
using Waveshare.Common;

#endregion Usings

namespace Waveshare.Devices.Epd7in5b_V2
{
    /// <summary>
    /// Write image to Waveshare 7.5inch e-Paper V2 Black, White and Red Display
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal class Epd7In5BV2Writer : EPaperDisplayWriter
    {
        //########################################################################################

        #region Fields

        private readonly int m_RedIndex;
        private readonly byte m_RedPixel;
        private readonly int m_BlackIndex;
        private readonly byte m_BlackPixel;
        private readonly byte[] m_BlackLine;
        private MemoryStream m_MemStream;
        private byte m_OutByte;

        #endregion Fields

        //########################################################################################

        #region Constructor / Dispose / Finalizer

        internal Epd7In5BV2Writer(EPaperDisplayBase display) 
            : base (display)
        {
            m_MemStream = new MemoryStream();
            m_RedIndex = display.GetColorIndex(ByteColors.Red);
            m_RedPixel = display.DeviceByteColors[m_RedIndex];
            m_BlackIndex = display.GetColorIndex(ByteColors.Black);
            m_BlackPixel = display.DeviceByteColors[m_BlackIndex];
            m_BlackLine = display.GetColoredLineOnDevice(ByteColors.Black);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && m_MemStream != null)
            {
                Finish();
                m_MemStream.Close();
                m_MemStream.Dispose();
                m_MemStream = null;
            }
        }

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

        #region Public Methods

        public override void Finish()
        {
            base.Finish();
            if (m_MemStream.Length > 0)
            {
                Display.SendCommand((byte)Epd7In5b_V2Commands.DataStartTransmission2);
                m_MemStream.Position = 0;
                Display.SendData(m_MemStream);
            }
            m_OutByte = 0;
        }

        public override void Write(int index)
        {
            byte value;
            if (index == m_RedIndex)
            {
                base.Write(m_BlackIndex);
                value = m_RedPixel;
            }
            else
            {
                base.Write(index);
                value = m_BlackPixel;
            }

            if (PixelPerByte == 1)
            {
                m_MemStream.WriteByte(value);
            }
            else
            {
                m_OutByte <<= BitShift;
                m_OutByte |= value;
                if (ByteCount % PixelPerByte == PixelThreshold)
                {
                    m_MemStream.WriteByte(m_OutByte);
                    m_OutByte = 0;
                }
            }
        }

        public override void WriteBlankLine()
        {
            while (PixelPerByte > 1 && ByteCount % PixelPerByte != PixelThreshold)
            {
                Write(WhiteIndex);
            }

            m_MemStream.Write(m_BlackLine, 0, m_BlackLine.Length);
            base.WriteBlankLine();
        }

        #endregion Public Methods
    }
}
