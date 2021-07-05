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

using System;
using System.IO;
using Waveshare.Interfaces.Internal;

#endregion Usings

namespace Waveshare.Common
{
    /// <summary>
    /// Write image to Waveshare E-Paper Display
    /// </summary>
    internal class EPaperDisplayWriter : IEPaperDisplayWriter
    {

        //########################################################################################

        #region Fields

        /// <summary>
        /// Threshold for a buffer flush
        /// </summary>
        private static readonly int StreamThreshold = 4096;

        /// <summary>
        /// White line on the device
        /// </summary>
        private readonly byte[] m_WhiteLine;

        /// <summary>
        /// Layer 1 Buffer for the pixels on the device
        /// </summary>
        private MemoryStream m_MemoryStream;

        /// <summary>
        /// Output Bytes
        /// </summary>
        private byte m_OutByte;

        /// <summary>
        /// Current Byte Count
        /// </summary>
        private int m_ByteCount = -1;

        #endregion Fields

        //########################################################################################

        #region Properties

        /// <summary>
        /// The Display device
        /// </summary>
        protected IEPaperDisplayInternal Display { get; }

        /// <summary>
        /// Current Byte Count in the Buffer
        /// </summary>
        protected int ByteCount => m_ByteCount;

        /// <summary>
        /// Pixel per Byte on the Device
        /// </summary>
        protected int PixelPerByte { get; }

        /// <summary>
        /// Pixel Threshold for PixelPerByte
        /// </summary>
        protected int PixelThreshold { get; }

        /// <summary>
        /// Bit Shift Value for PixelPerByte
        /// </summary>
        protected byte BitShift { get; }

        /// <summary>
        /// Pallet color index on device for White
        /// </summary>
        protected int WhiteIndex { get; }

        #endregion Properties

        //########################################################################################

        #region Constructor / Dispose / Finalizer

        /// <summary>
        /// Constructor with E-Paper Display
        /// </summary>
        /// <param name="display"></param>
        public EPaperDisplayWriter(IEPaperDisplayInternal display)
        {
            Display = display;
            PixelPerByte = display.PixelPerByte;
            BitShift = (byte)(8 / PixelPerByte);
            PixelThreshold = PixelPerByte - 1;
            WhiteIndex = display.GetColorIndex(ByteColors.White);
            m_WhiteLine = display.GetColoredLineOnDevice(ByteColors.White);
            m_MemoryStream = new MemoryStream();
        }

        /// <summary>
        /// Protected Dispose for derived classes
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && m_MemoryStream != null)
            {
                Finish();
                m_MemoryStream.Close();
                m_MemoryStream.Dispose();
                m_MemoryStream = null;
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~EPaperDisplayWriter() => Dispose(false);

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Send the Data to the Hardware
        /// </summary>
        public virtual void Finish()
        {
            Flush();
            m_OutByte = 0;
            m_ByteCount = -1;
        }

        /// <summary>
        /// Write to the Buffer
        /// </summary>
        /// <param name="index"></param>
        public virtual void Write(int index)
        {
            var value = Display.DeviceByteColors[index];
            m_ByteCount++;

            if (PixelPerByte == 1)
            {
                m_MemoryStream.WriteByte(value);
                if (m_MemoryStream.Length >= StreamThreshold)
                {
                    Flush();
                }
            }
            else
            {
                m_OutByte <<= BitShift;
                m_OutByte |= value;

                if (m_ByteCount % PixelPerByte == PixelThreshold)
                {
                    m_MemoryStream.WriteByte(m_OutByte);
                    m_OutByte = 0;
                    if (m_MemoryStream.Length >= StreamThreshold)
                    {
                        Flush();
                    }
                }
            }
        }

        /// <summary>
        /// Write a Blank Line in the Buffer
        /// </summary>
        public virtual void WriteBlankLine()
        {
            while (PixelPerByte > 1 && m_ByteCount % PixelPerByte != PixelThreshold)
            {
                Write(WhiteIndex);
            }

            m_MemoryStream.Write(m_WhiteLine, 0, m_WhiteLine.Length);
        }

        /// <summary>
        /// Write a Blank Pixel
        /// </summary>
        public virtual void WriteBlankPixel()
        {
            Write(WhiteIndex);
        }

        #endregion Public Methods

        //########################################################################################

        #region Protected Methods

        /// <summary>
        /// Send the Data to the Hardware and clear the Stream
        /// </summary>
        protected virtual void Flush()
        {
            if (m_MemoryStream != null && m_MemoryStream.Length > 0)
            {
                m_MemoryStream.Position = 0;
                Display.SendData(m_MemoryStream);
                m_MemoryStream.SetLength(0);
            }
        }

        #endregion Protected Methods

        //########################################################################################

    }
}
