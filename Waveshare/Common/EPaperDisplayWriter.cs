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

#endregion Usings

namespace Waveshare.Common
{
    /// <summary>
    /// Write image to Waveshare E-Paper Display
    /// </summary>
    public class EPaperDisplayWriter : IDisposable
    {
        //########################################################################################

        #region Fields

        internal readonly EPaperDisplayBase Display;
        protected readonly int Width;
        protected readonly int PixelPerByte;
        protected readonly int PixelThreshold;
        protected readonly int StreamThreshold = 4096;
        protected readonly byte BitShift;
        protected readonly int WhiteIndex;
        protected readonly byte[] WhiteLine;
        private MemoryStream memStream;
        private byte OutByte;
        private int m_ByteCount = -1;
        private bool Disposed = false;

        #endregion Fields

        //########################################################################################

        #region Properties

        protected int ByteCount => m_ByteCount;

        #endregion Properties

        //########################################################################################

        #region Constructor / Dispose / Finalizer

        internal EPaperDisplayWriter(EPaperDisplayBase display)
        {
            Display = display;
            Width = display.Width;
            PixelPerByte = display.PixelPerByte;
            BitShift = (byte)(8 / PixelPerByte);
            PixelThreshold = PixelPerByte - 1;
            memStream = new MemoryStream();
            WhiteIndex = display.GetColorIndex(ByteColors.White);
            WhiteLine = display.GetColoredLineOnDevice(ByteColors.White);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing && memStream != null)
            {
                Finish();
                memStream.Close();
                memStream.Dispose();
                memStream = null;
            }

            Disposed = true;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EPaperDisplayWriter() => Dispose(false);

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

        #region Public Methods

        public virtual void Finish()
        {
            Flush();
            OutByte = 0;
            m_ByteCount = -1;
        }

        public virtual void Flush()
        {
            if (memStream != null && memStream.Length > 0)
            {
                memStream.Position = 0;
                Display.SendData(memStream);
                memStream.SetLength(0);
            }
        }

        public virtual void Write(int index)
        {
            byte value = Display.DeviceByteColors[index];
            m_ByteCount++;
            if (PixelPerByte == 1)
            {
                memStream.WriteByte(value);
                if (memStream.Length >= StreamThreshold)
                {
                    Flush();
                }
            }
            else
            {
                OutByte <<= BitShift;
                OutByte |= value;
                if (m_ByteCount % PixelPerByte == PixelThreshold)
                {
                    memStream.WriteByte(OutByte);
                    OutByte = 0;
                    if (memStream.Length >= StreamThreshold)
                    {
                        Flush();
                    }
                }
            }
        }

        public virtual void Write(int[] indexes)
        {
            foreach (int index in indexes)
            {
                Write(index);
            }
        }

        public virtual void WriteBlankLine()
        {
            while (PixelPerByte > 1 && m_ByteCount % PixelPerByte != PixelThreshold)
            {
                Write(WhiteIndex);
            }

            memStream.Write(WhiteLine, 0, WhiteLine.Length);
        }

        public virtual void WriteBlankPixel()
        {
            Write(WhiteIndex);
        }

        #endregion Public Methods

        //########################################################################################
    }
}
