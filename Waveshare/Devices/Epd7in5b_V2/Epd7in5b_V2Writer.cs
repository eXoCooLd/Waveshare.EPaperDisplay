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
using Waveshare.Interfaces.Internal;

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

        /// <summary>
        /// Pallet color index on device for Red
        /// </summary>
        private readonly int m_RedIndex;

        /// <summary>
        /// Byte value for color Red
        /// </summary>
        private readonly byte m_RedPixel;

        /// <summary>
        /// Pallet color index on device for Black
        /// </summary>
        private readonly int m_BlackIndex;

        /// <summary>
        /// Byte value for color black
        /// </summary>
        private readonly byte m_BlackPixel;

        /// <summary>
        /// Black line on the device
        /// </summary>
        private readonly byte[] m_BlackLine;

        /// <summary>
        /// Layer 2 Buffer for the pixels on the device
        /// </summary>
        private MemoryStream m_Layer2MemoryStream;

        /// <summary>
        /// Output bytes
        /// </summary>
        private byte m_OutByte;

        #endregion Fields

        //########################################################################################

        #region Constructor / Dispose / Finalizer

        /// <summary>
        /// Constructor with E-Paper Display
        /// </summary>
        /// <param name="display"></param>
        public Epd7In5BV2Writer(IEPaperDisplayInternal display) 
            : base (display)
        {
            m_Layer2MemoryStream = new MemoryStream();
            m_RedIndex = display.GetColorIndex(ByteColors.Red);
            m_RedPixel = display.DeviceByteColors[m_RedIndex];
            m_BlackIndex = display.GetColorIndex(ByteColors.Black);
            m_BlackPixel = display.DeviceByteColors[m_BlackIndex];
            m_BlackLine = display.GetColoredLineOnDevice(ByteColors.Black);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && m_Layer2MemoryStream != null)
            {
                Finish();
                m_Layer2MemoryStream.Close();
                m_Layer2MemoryStream.Dispose();
                m_Layer2MemoryStream = null;
            }
        }

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Send the Data to the Hardware
        /// </summary>
        public override void Finish()
        {
            base.Finish();
            if (m_Layer2MemoryStream.Length > 0)
            {
                Display.SendCommand((byte)Epd7In5b_V2Commands.DataStartTransmission2);
                m_Layer2MemoryStream.Position = 0;
                Display.SendData(m_Layer2MemoryStream);
            }
            m_OutByte = 0;
        }

        /// <summary>
        /// Write to the Buffer
        /// </summary>
        /// <param name="index"></param>
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
                m_Layer2MemoryStream.WriteByte(value);
            }
            else
            {
                m_OutByte <<= BitShift;
                m_OutByte |= value;
                if (ByteCount % PixelPerByte == PixelThreshold)
                {
                    m_Layer2MemoryStream.WriteByte(m_OutByte);
                    m_OutByte = 0;
                }
            }
        }

        /// <summary>
        /// Write a Blank Line in the Buffer
        /// </summary>
        public override void WriteBlankLine()
        {
            while (PixelPerByte > 1 && ByteCount % PixelPerByte != PixelThreshold)
            {
                Write(WhiteIndex);
            }

            m_Layer2MemoryStream.Write(m_BlackLine, 0, m_BlackLine.Length);
            base.WriteBlankLine();
        }

        #endregion Public Methods

        //########################################################################################

    }
}
